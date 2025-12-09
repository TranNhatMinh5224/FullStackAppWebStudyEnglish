using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Cofigurations;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Service.Auth
{
    public class FacebookLoginService : IFacebookLoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IExternalLoginRepository _externalLoginRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly ILogger<FacebookLoginService> _logger;
        private readonly FacebookAuthOptions _facebookAuthOptions;
        private readonly HttpClient _httpClient;
        private readonly IMinioFileStorage _minioService;
        private const string AVATAR_BUCKET_NAME = "avatar";
        private const string FOLDERTEMP = "temp";

        public FacebookLoginService(
            IUserRepository userRepository,
            IExternalLoginRepository externalLoginRepository,
            IMapper mapper,
            ITokenService tokenService,
            ILogger<FacebookLoginService> logger,
            IOptions<FacebookAuthOptions> facebookAuthOptions,
            IHttpClientFactory httpClientFactory,
            IMinioFileStorage minioService)
        {
            _userRepository = userRepository;
            _externalLoginRepository = externalLoginRepository;
            _mapper = mapper;
            _tokenService = tokenService;
            _logger = logger;
            _facebookAuthOptions = facebookAuthOptions.Value;
            _httpClient = httpClientFactory.CreateClient();
            _minioService = minioService;
        }

        public async Task<ServiceResponse<AuthResponseDto>> HandleFacebookLoginAsync(FacebookLoginDto facebookLoginDto)
        {
            var response = new ServiceResponse<AuthResponseDto>();
            try
            {
                // Bước 1: Verify Facebook Access Token
                var appId = _facebookAuthOptions.AppId;
                var appSecret = _facebookAuthOptions.AppSecret;

                var verifyUrl = $"https://graph.facebook.com/debug_token?input_token={facebookLoginDto.AccessToken}&access_token={appId}|{appSecret}";
                var verifyResponse = await _httpClient.GetAsync(verifyUrl);

                if (!verifyResponse.IsSuccessStatusCode)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Token Facebook không hợp lệ";
                    return response;
                }

                var verifyJson = await verifyResponse.Content.ReadAsStringAsync();
                var verifyData = JsonSerializer.Deserialize<FacebookTokenVerification>(verifyJson);

                if (verifyData?.Data?.IsValid != true)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Token Facebook đã hết hạn hoặc không hợp lệ";
                    return response;
                }

                // Bước 2: Lấy thông tin user từ Facebook
                var userInfoUrl = $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,picture&access_token={facebookLoginDto.AccessToken}";
                var userInfoResponse = await _httpClient.GetAsync(userInfoUrl);

                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Không thể lấy thông tin từ Facebook";
                    return response;
                }

                var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
                var facebookUser = JsonSerializer.Deserialize<FacebookUserInfo>(userInfoJson);

                if (string.IsNullOrEmpty(facebookUser?.Email))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Email không được cung cấp bởi Facebook. Vui lòng cấp quyền email.";
                    return response;
                }

                // Bước 3: Kiểm tra xem đã từng đăng nhập bằng Facebook chưa
                var existingExternalLogin = await _externalLoginRepository
                    .GetByProviderAndUserIdAsync("Facebook", facebookUser.Id);

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
                    user = await _userRepository.GetUserByEmailAsync(facebookUser.Email);

                    if (user == null)
                    {
                        // Download và upload avatar lên MinIO
                        string? avatarKey = null;
                        var avatarUrl = facebookUser.Picture?.Data?.Url;
                        if (!string.IsNullOrEmpty(avatarUrl))
                        {
                            avatarKey = await DownloadAndUploadAvatarAsync(avatarUrl, facebookUser.Email);
                        }

                        // Tạo User mới
                        user = new User
                        {
                            Email = facebookUser.Email,
                            FirstName = facebookUser.FirstName ?? "",
                            LastName = facebookUser.LastName ?? "",
                            EmailVerified = true, // Facebook email đã verified
                            NormalizedEmail = facebookUser.Email.ToUpper(),
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

                    // Tạo External Login mới
                    var newExternalLogin = new ExternalLogin
                    {
                        Provider = "Facebook",
                        ProviderUserId = facebookUser.Id,
                        ProviderDisplayName = $"{facebookUser.FirstName} {facebookUser.LastName}",
                        ProviderEmail = facebookUser.Email,
                        ProviderPhotoUrl = facebookUser.Picture?.Data?.Url,
                        UserId = user.UserId,
                        CreatedAt = DateTime.UtcNow,
                        LastUsedAt = DateTime.UtcNow
                    };

                    await _externalLoginRepository.AddAsync(newExternalLogin);
                    await _externalLoginRepository.SaveChangesAsync();
                }

                // Bước 4: Kiểm tra trạng thái tài khoản
                if (user.Status == Domain.Enums.AccountStatus.Inactive)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Tài khoản của bạn đã bị khóa do vi phạm chính sách!";
                    return response;
                }

                // Bước 5: Tạo JWT token
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Đăng nhập bằng Facebook thành công";
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
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình đăng nhập Facebook");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống khi đăng nhập bằng Facebook";
            }

            return response;
        }

        // Helper classes for Facebook API responses
        private class FacebookTokenVerification
        {
            [JsonPropertyName("data")]
            public TokenData? Data { get; set; }
        }

        private class TokenData
        {
            [JsonPropertyName("is_valid")]
            public bool IsValid { get; set; }

            [JsonPropertyName("user_id")]
            public string? UserId { get; set; }
        }

        private class FacebookUserInfo
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("first_name")]
            public string? FirstName { get; set; }

            [JsonPropertyName("last_name")]
            public string? LastName { get; set; }

            [JsonPropertyName("picture")]
            public PictureData? Picture { get; set; }
        }

        private class PictureData
        {
            [JsonPropertyName("data")]
            public ImageData? Data { get; set; }
        }

        private class ImageData
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }
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
