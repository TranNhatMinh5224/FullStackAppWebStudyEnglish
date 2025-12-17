using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Service.Auth
{
    /// <summary>
    /// Service xử lý đăng nhập bằng Facebook OAuth2 - Application Layer
    /// Chứa business logic: tạo user, validate, generate JWT
    /// Delegate HTTP calls cho IFacebookAuthProvider (Infrastructure layer)
    /// </summary>
    public class FacebookLoginService : IFacebookLoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IExternalLoginRepository _externalLoginRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly ILogger<FacebookLoginService> _logger;
        private readonly IFacebookAuthProvider _facebookAuthProvider;
        private readonly IMinioFileStorage _minioService;
        private const string AVATAR_BUCKET_NAME = "avatars";
        private const string FOLDERREAL = "real";

        public FacebookLoginService(
            IUserRepository userRepository,
            IExternalLoginRepository externalLoginRepository,
            IMapper mapper,
            ITokenService tokenService,
            ILogger<FacebookLoginService> logger,
            IFacebookAuthProvider facebookAuthProvider,
            IMinioFileStorage minioService)
        {
            _userRepository = userRepository;
            _externalLoginRepository = externalLoginRepository;
            _mapper = mapper;
            _tokenService = tokenService;
            _logger = logger;
            _facebookAuthProvider = facebookAuthProvider;
            _minioService = minioService;
        }

        // Xử lý đăng nhập bằng Facebook OAuth2 (Business Logic Only)
        public async Task<ServiceResponse<AuthResponseDto>> HandleFacebookLoginAsync(FacebookLoginDto facebookLoginDto)
        {
            var response = new ServiceResponse<AuthResponseDto>();
            try
            {
                // Validate CSRF state parameter
                if (string.IsNullOrEmpty(facebookLoginDto.State))
                {
                    _logger.LogWarning("Facebook login attempt without CSRF state parameter");
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "State parameter là bắt buộc để bảo mật CSRF";
                    return response;
                }

                // Validate authorization code
                if (string.IsNullOrEmpty(facebookLoginDto.Code))
                {
                    _logger.LogWarning("Facebook login attempt without authorization code");
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Authorization code là bắt buộc";
                    return response;
                }

                // Delegate HTTP call to Infrastructure layer
                var facebookUser = await _facebookAuthProvider.GetUserInfoFromCodeAsync(facebookLoginDto.Code);
                if (facebookUser == null)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Không thể xác thực với Facebook. Vui lòng thử lại.";
                    return response;
                }

                // Business Logic: Get or create user
                var user = await GetOrCreateUserAsync(facebookUser);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 409;
                    response.Message = "Email này đã được đăng ký bằng phương thức khác. Vui lòng sử dụng phương thức đăng nhập ban đầu.";
                    return response;
                }

                // Check account status
                if (user.Status == Domain.Enums.AccountStatus.Inactive)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Tài khoản của bạn đã bị khóa do vi phạm chính sách!";
                    return response;
                }

                // Generate JWT tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Đăng nhập bằng Facebook thành công";

                var userDto = _mapper.Map<UserDto>(user);

                // Build public URL for avatar
                if (!string.IsNullOrWhiteSpace(user.AvatarKey))
                {
                    userDto.AvatarUrl = BuildPublicUrl.BuildURL(AVATAR_BUCKET_NAME, user.AvatarKey);
                }

                response.Data = new AuthResponseDto
                {
                    AccessToken = accessToken.Item1,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = accessToken.Item2,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình đăng nhập Facebook");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống khi đăng nhập bằng Facebook";
            }

            return response;
        }

        /// <summary>
        /// Business Logic: Tạo hoặc lấy user từ thông tin Facebook
        /// </summary>
        private async Task<User?> GetOrCreateUserAsync(FacebookUserInfo facebookUser)
        {
            // Tìm external login đã tồn tại
            var existingExternalLogin = await _externalLoginRepository
                .GetByProviderAndUserIdAsync("Facebook", facebookUser.Id);

            if (existingExternalLogin != null)
            {
                // Đã có tài khoản Facebook - update last used time
                var user = await _userRepository.GetByIdAsync(existingExternalLogin.UserId);
                if (user != null)
                {
                    existingExternalLogin.LastUsedAt = DateTime.UtcNow;
                    await _externalLoginRepository.SaveChangesAsync();
                }
                return user;
            }

            // Chưa có external login - check email exists
            var email = facebookUser.Email ?? $"facebook_{facebookUser.Id}@noemail.local";
            var existingUser = await _userRepository.GetUserByEmailAsync(email);
            
            if (existingUser != null)
            {
                // Email đã tồn tại với phương thức khác
                _logger.LogWarning("Attempt to login with Facebook for existing email: {Email}", email);
                return null; // Signal conflict
            }

            // Tạo user mới
            var newUser = await CreateNewUserFromFacebookAsync(facebookUser);

            // Tạo external login record
            var newExternalLogin = new ExternalLogin
            {
                Provider = "Facebook",
                ProviderUserId = facebookUser.Id,
                ProviderDisplayName = $"{facebookUser.FirstName} {facebookUser.LastName}",
                ProviderEmail = email,
                ProviderPhotoUrl = facebookUser.PictureUrl,
                UserId = newUser.UserId,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };

            await _externalLoginRepository.AddAsync(newExternalLogin);
            await _externalLoginRepository.SaveChangesAsync();

            return newUser;
        }

        /// <summary>
        /// Tạo user mới từ thông tin Facebook
        /// </summary>
        private async Task<User> CreateNewUserFromFacebookAsync(FacebookUserInfo facebookUser)
        {
            var email = facebookUser.Email ?? $"facebook_{facebookUser.Id}@noemail.local";

            // Download và upload avatar nếu có
            string? avatarKey = null;
            if (!string.IsNullOrEmpty(facebookUser.PictureUrl))
            {
                avatarKey = await DownloadAndUploadAvatarAsync(facebookUser.PictureUrl, email);
            }

            var user = new User
            {
                Email = email,
                FirstName = facebookUser.FirstName ?? "",
                LastName = facebookUser.LastName ?? "",
                EmailVerified = !string.IsNullOrEmpty(facebookUser.Email), // True nếu có email thật
                NormalizedEmail = email.ToUpper(),
                AvatarKey = avatarKey,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Assign default Student role
            var studentRole = await _userRepository.GetRoleByNameAsync("Student");
            if (studentRole != null)
            {
                user.Roles.Add(studentRole);
            }

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Download avatar từ Facebook và upload lên MinIO
        /// </summary>
        private async Task<string?> DownloadAndUploadAvatarAsync(string avatarUrl, string userEmail)
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageResponse = await httpClient.GetAsync(avatarUrl);
                if (!imageResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Không thể tải avatar từ URL: {Url}", avatarUrl);
                    return null;
                }

                var imageStream = await imageResponse.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var fileName = $"{userEmail}_{Guid.NewGuid()}.jpg";
                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "avatar", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                };

                var uploadResult = await _minioService.UpLoadFileTempAsync(formFile, AVATAR_BUCKET_NAME, FOLDERREAL);

                if (uploadResult.Success && uploadResult.Data != null)
                {
                    _logger.LogInformation("Upload avatar thành công cho user: {Email}", userEmail);
                    return uploadResult.Data.TempKey;
                }
                else
                {
                    _logger.LogWarning("Upload avatar lên MinIO thất bại cho user: {Email}", userEmail);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi download/upload avatar từ URL: {Url}", avatarUrl);
                return null;
            }
        }
    }
}
