using AutoMapper;
using Google.Apis.Auth;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Cofigurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Service
{
    public class GoogleLoginService : IGoogleLoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IExternalLoginRepository _externalLoginRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly ILogger<GoogleLoginService> _logger;
        private readonly GoogleAuthOptions _googleAuthOptions;
        private readonly IMinioFileStorage _minioService;
        private readonly HttpClient _httpClient;
        private const string AVATAR_BUCKET_NAME = "avatar";
        private const string FOLDERTEMP = "temp";

        public GoogleLoginService(
            IUserRepository userRepository,
            IExternalLoginRepository externalLoginRepository,
            IMapper mapper,
            ITokenService tokenService,
            ILogger<GoogleLoginService> logger,
            IOptions<GoogleAuthOptions> googleAuthOptions,
            IMinioFileStorage minioService,
            IHttpClientFactory httpClientFactory)
        {
            _userRepository = userRepository;
            _externalLoginRepository = externalLoginRepository;
            _mapper = mapper;
            _tokenService = tokenService;
            _logger = logger;
            _googleAuthOptions = googleAuthOptions.Value;
            _minioService = minioService;
            _httpClient = httpClientFactory.CreateClient();
        }

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

                // Verify token với Google
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(
                        googleLoginDto.IdToken,
                        new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[] { _googleAuthOptions.ClientId }
                        });
                }
                catch (InvalidJwtException)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Token Google không hợp lệ";
                    return response;
                }

                if (payload == null || string.IsNullOrEmpty(payload.Email))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Không thể lấy thông tin email từ Google";
                    return response;
                }

                // Bước 2: Kiểm tra xem đã từng đăng nhập bằng Google chưa
                var existingExternalLogin = await _externalLoginRepository
                    .GetByProviderAndUserIdAsync("Google", payload.Subject);

                User? user;

                if (existingExternalLogin != null)
                {
                    // Đã có External Login → lấy User
                    user = await _userRepository.GetByIdAsync(existingExternalLogin.UserId);

                    if (user == null)
                    {
                        response.Success = false;
                        response.StatusCode = 404;
                        response.Message = "Không tìm thấy người dùng";
                        return response;
                    }

                    // Cập nhật LastUsedAt
                    existingExternalLogin.LastUsedAt = DateTime.UtcNow;
                    await _externalLoginRepository.SaveChangesAsync();
                }
                else
                {
                    // Chưa có External Login → kiểm tra email có tồn tại không
                    user = await _userRepository.GetUserByEmailAsync(payload.Email);

                    if (user == null)
                    {
                        // Download và upload avatar lên MinIO
                        string? avatarKey = null;
                        if (!string.IsNullOrEmpty(payload.Picture))
                        {
                            avatarKey = await DownloadAndUploadAvatarAsync(payload.Picture, payload.Email);
                        }

                        // Tạo User mới
                        user = new User
                        {
                            Email = payload.Email,
                            FirstName = payload.GivenName ?? "",
                            LastName = payload.FamilyName ?? "",
                            EmailVerified = payload.EmailVerified,
                            NormalizedEmail = payload.Email.ToUpper(),
                            AvatarKey = avatarKey,  // Lưu MinIO key thay vì URL
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        // Lấy role Student mặc định
                        var studentRole = await _userRepository.GetRoleByNameAsync("Student");
                        if (studentRole != null)
                        {
                            user.Roles.Add(studentRole);
                        }

                        await _userRepository.AddUserAsync(user);
                        await _userRepository.SaveChangesAsync();
                    }
                    else
                    {
                        // Email đã tồn tại với phương thức khác - chặn cross-login
                        _logger.LogWarning("Attempt to login with Google for existing email with different method. Email: {Email}", payload.Email);
                        
                        response.Success = false;
                        response.StatusCode = 409;
                        response.Message = "Email này đã được đăng ký bằng phương thức khác. Vui lòng sử dụng phương thức đăng nhập ban đầu.";
                        return response;
                    }

                    // Tạo External Login mới
                    var newExternalLogin = new ExternalLogin
                    {
                        Provider = "Google",
                        ProviderUserId = payload.Subject,
                        ProviderDisplayName = payload.Name,
                        ProviderEmail = payload.Email,
                        ProviderPhotoUrl = payload.Picture,
                        UserId = user.UserId,
                        CreatedAt = DateTime.UtcNow,
                        LastUsedAt = DateTime.UtcNow
                    };

                    await _externalLoginRepository.AddAsync(newExternalLogin);
                    await _externalLoginRepository.SaveChangesAsync();
                }

                // Kiểm tra trạng thái tài khoản
                if (user.Status == Domain.Enums.AccountStatus.Inactive)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Tài khoản của bạn đã bị khóa do vi phạm chính sách!";
                    return response;
                }

                // Tạo JWT token
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Đăng nhập bằng Google thành công";
                response.Data = new AuthResponseDto
                {
                    AccessToken = accessToken.Item1,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = accessToken.Item2,
                    User = _mapper.Map<UserDto>(user)
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

        private async Task<string?> DownloadAndUploadAvatarAsync(string avatarUrl, string userEmail)
        {
            try
            {
                // Download ảnh từ URL
                var imageResponse = await _httpClient.GetAsync(avatarUrl);
                if (!imageResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Không thể tải avatar từ URL: {Url}", avatarUrl);
                    return null;
                }

                var imageStream = await imageResponse.Content.ReadAsStreamAsync();

                // Convert sang IFormFile để upload lên MinIO
                var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var fileName = $"{userEmail}_{Guid.NewGuid()}.jpg";
                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "avatar", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                };

                // Upload lên MinIO
                var uploadResult = await _minioService.UpLoadFileTempAsync(formFile, AVATAR_BUCKET_NAME, FOLDERTEMP);

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