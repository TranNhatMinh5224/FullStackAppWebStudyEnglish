using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Auth;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Service
{
    // Service xử lý đăng nhập bằng Google OAuth2
    // Chứa business logic: tạo user, validate, generate JWT
    // Delegate HTTP calls cho IGoogleAuthProvider (Infrastructure layer)
    public class GoogleLoginService : IGoogleLoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IExternalLoginRepository _externalLoginRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly ILogger<GoogleLoginService> _logger;
        private readonly IGoogleAuthProvider _googleAuthProvider;
        private readonly IAvatarService _avatarService;

        public GoogleLoginService(
            IUserRepository userRepository,
            IExternalLoginRepository externalLoginRepository,
            IMapper mapper,
            ITokenService tokenService,
            ILogger<GoogleLoginService> logger,
            IGoogleAuthProvider googleAuthProvider,
            IAvatarService avatarService)
        {
            _userRepository = userRepository;
            _externalLoginRepository = externalLoginRepository;
            _mapper = mapper;
            _tokenService = tokenService;
            _logger = logger;
            _googleAuthProvider = googleAuthProvider;
            _avatarService = avatarService;
        }

        // Xử lý đăng nhập bằng Google OAuth2 (Business Logic Only)
        public async Task<ServiceResponse<AuthResponseDto>> HandleGoogleLoginAsync(GoogleLoginDto googleLoginDto)
        {
            var response = new ServiceResponse<AuthResponseDto>();
            try
            {
                // Validate CSRF state parameter
                if (string.IsNullOrEmpty(googleLoginDto.State))
                {
                    _logger.LogWarning("Google login attempt without CSRF state parameter");
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "State parameter là bắt buộc để bảo mật CSRF";
                    return response;
                }

                // Validate authorization code
                if (string.IsNullOrEmpty(googleLoginDto.Code))
                {
                    _logger.LogWarning("Google login attempt without authorization code");
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Authorization code là bắt buộc";
                    return response;
                }

                // Delegate HTTP call to Infrastructure layer
                var googleUser = await _googleAuthProvider.GetUserInfoFromCodeAsync(googleLoginDto.Code);
                if (googleUser == null)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Không thể xác thực với Google. Vui lòng thử lại.";
                    return response;
                }

                // Validate email
                if (string.IsNullOrEmpty(googleUser.Email))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Không thể lấy thông tin email từ Google";
                    return response;
                }

                // Business Logic: Get or create user
                var user = await GetOrCreateUserAsync(googleUser);
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
                response.Message = "Đăng nhập bằng Google thành công";

                var userDto = _mapper.Map<UserDto>(user);

                // Build public URL for avatar
                if (!string.IsNullOrWhiteSpace(user.AvatarKey))
                {
                    userDto.AvatarUrl = _avatarService.BuildAvatarUrl(user.AvatarKey);
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
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình đăng nhập Google");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống khi đăng nhập bằng Google";
            }

            return response;
        }

        // Business Logic: Tạo hoặc lấy user từ thông tin Google
        private async Task<User?> GetOrCreateUserAsync(GoogleUserInfo googleUser)
        {
            // Tìm external login đã tồn tại
            var existingExternalLogin = await _externalLoginRepository
                .GetByProviderAndUserIdAsync("Google", googleUser.Subject);

            if (existingExternalLogin != null)
            {
                // Đã có tài khoản Google - update last used time
                var user = await _userRepository.GetByIdAsync(existingExternalLogin.UserId);
                if (user != null)
                {
                    existingExternalLogin.LastUsedAt = DateTime.UtcNow;
                    await _externalLoginRepository.SaveChangesAsync();
                }
                return user;
            }

            // Chưa có external login - check email exists
            var existingUser = await _userRepository.GetUserByEmailAsync(googleUser.Email);
            if (existingUser != null)
            {
                // Email đã tồn tại với phương thức khác
                _logger.LogWarning("Attempt to login with Google for existing email: {Email}", googleUser.Email);
                return null; // Signal conflict
            }

            // Tạo user mới
            var newUser = await CreateNewUserFromGoogleAsync(googleUser);
            
            // Tạo external login record
            var newExternalLogin = new ExternalLogin
            {
                Provider = "Google",
                ProviderUserId = googleUser.Subject,
                ProviderDisplayName = googleUser.Name,
                ProviderEmail = googleUser.Email,
                ProviderPhotoUrl = googleUser.Picture,
                UserId = newUser.UserId,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };

            await _externalLoginRepository.AddAsync(newExternalLogin);
            await _externalLoginRepository.SaveChangesAsync();

            return newUser;
        }

        // Tạo user mới từ thông tin Google
        private async Task<User> CreateNewUserFromGoogleAsync(GoogleUserInfo googleUser)
        {
            // Download và upload avatar nếu có
            string? avatarKey = null;
            if (!string.IsNullOrEmpty(googleUser.Picture))
            {
                avatarKey = await DownloadAndUploadAvatarAsync(googleUser.Picture, googleUser.Email);
            }

            var user = new User
            {
                Email = googleUser.Email,
                FirstName = googleUser.GivenName ?? "",
                LastName = googleUser.FamilyName ?? "",
                EmailVerified = googleUser.EmailVerified,
                NormalizedEmail = googleUser.Email.ToUpper(),
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

        // Download avatar từ Google và upload lên MinIO
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

                // Sử dụng AvatarService để upload - không cần biết bucket/folder
                var uploadResult = await _avatarService.UploadTempAvatarAsync(formFile);

                if (uploadResult.Success && !string.IsNullOrWhiteSpace(uploadResult.Data))
                {
                    _logger.LogInformation("Upload avatar thành công cho user: {Email}", userEmail);
                    return uploadResult.Data;
                }
                else
                {
                    _logger.LogWarning("Upload avatar lên MinIO thất bại cho user: {Email}", userEmail);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải xuống/upload avatar từ URL: {Url}", avatarUrl);
                return null;
            }
        }
    }
}