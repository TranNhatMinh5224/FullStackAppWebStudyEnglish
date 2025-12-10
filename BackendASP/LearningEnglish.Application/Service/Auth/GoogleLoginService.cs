using AutoMapper;
using Google.Apis.Auth;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Cofigurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Service
{
    // Service xử lý đăng nhập bằng Google OAuth
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
        private const string AVATAR_BUCKET_NAME = "avatars";
        private const string FOLDERREAL = "real";

        // Constructor khởi tạo các dependency injection
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

        // Xử lý đăng nhập bằng Google OAuth với bảo mật CSRF
        public async Task<ServiceResponse<AuthResponseDto>> HandleGoogleLoginAsync(GoogleLoginDto googleLoginDto)
        {
            var response = new ServiceResponse<AuthResponseDto>();
            try
            {
                // Kiểm tra tham số CSRF state để bảo mật
                if (string.IsNullOrEmpty(googleLoginDto.State))
                {
                    _logger.LogWarning("Google login attempt without CSRF state parameter");
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "State parameter là bắt buộc để bảo mật CSRF";
                    return response;
                }

                // Xác thực ID token với Google API
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

                // Kiểm tra thông tin email từ Google payload
                if (payload == null || string.IsNullOrEmpty(payload.Email))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Không thể lấy thông tin email từ Google";
                    return response;
                }

                // Tìm kiếm thông tin đăng nhập Google đã có
                var existingExternalLogin = await _externalLoginRepository
                    .GetByProviderAndUserIdAsync("Google", payload.Subject);

                User? user;

                if (existingExternalLogin != null)
                {
                    // Đã có tài khoản Google - lấy thông tin người dùng
                    user = await _userRepository.GetByIdAsync(existingExternalLogin.UserId);

                    if (user == null)
                    {
                        response.Success = false;
                        response.StatusCode = 404;
                        response.Message = "Không tìm thấy người dùng";
                        return response;
                    }

                    // Cập nhật thời gian đăng nhập gần nhất
                    existingExternalLogin.LastUsedAt = DateTime.UtcNow;
                    await _externalLoginRepository.SaveChangesAsync();
                }
                else
                {
                    // Chưa có tài khoản Google - kiểm tra email đã tồn tại chưa
                    user = await _userRepository.GetUserByEmailAsync(payload.Email);

                    if (user == null)
                    {
                        // Tải xuống và upload avatar lên MinIO
                        string? avatarKey = null;
                        if (!string.IsNullOrEmpty(payload.Picture))
                        {
                            avatarKey = await DownloadAndUploadAvatarAsync(payload.Picture, payload.Email);
                        }

                        // Tạo tài khoản người dùng mới từ thông tin Google
                        user = new User
                        {
                            Email = payload.Email,
                            FirstName = payload.GivenName ?? "",
                            LastName = payload.FamilyName ?? "",
                            EmailVerified = payload.EmailVerified,
                            NormalizedEmail = payload.Email.ToUpper(),
                            AvatarKey = avatarKey,  // Lưu MinIO key
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        // Gán quyền Student mặc định cho tài khoản mới
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
                        // Email đã tồn tại - ngăn chặn đăng nhập chéo phương thức
                        _logger.LogWarning("Attempt to login with Google for existing email with different method. Email: {Email}", payload.Email);
                        
                        response.Success = false;
                        response.StatusCode = 409;
                        response.Message = "Email này đã được đăng ký bằng phương thức khác. Vui lòng sử dụng phương thức đăng nhập ban đầu.";
                        return response;
                    }

                    // Tạo bản ghi đăng nhập Google mới
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

                // Kiểm tra trạng thái hoạt động của tài khoản
                if (user.Status == Domain.Enums.AccountStatus.Inactive)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Tài khoản của bạn đã bị khóa do vi phạm chính sách!";
                    return response;
                }

                // Tạo JWT access token và refresh token
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Đăng nhập bằng Google thành công";
                
                var userDto = _mapper.Map<UserDto>(user);
                
                // Tạo URL công khai cho avatar nếu có
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
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình đăng nhập Google");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống khi đăng nhập bằng Google";
            }

            return response;
        }

        // Tải xuống ảnh đại diện từ Google và upload lên MinIO
        private async Task<string?> DownloadAndUploadAvatarAsync(string avatarUrl, string userEmail)
        {
            try
            {
                // Tải ảnh từ URL Google cung cấp
                var imageResponse = await _httpClient.GetAsync(avatarUrl);
                if (!imageResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Không thể tải avatar từ URL: {Url}", avatarUrl);
                    return null;
                }

                var imageStream = await imageResponse.Content.ReadAsStreamAsync();

                // Chuyển đổi stream thành IFormFile để upload
                var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var fileName = $"{userEmail}_{Guid.NewGuid()}.jpg";
                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "avatar", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                };

                // Upload trực tiếp vào thư mục thực (không qua temp)
                var uploadResult = await _minioService.UpLoadFileTempAsync(formFile, AVATAR_BUCKET_NAME, FOLDERREAL);

                if (uploadResult.Success && uploadResult.Data != null)
                {
                    _logger.LogInformation("Upload avatar thành công cho user: {Email}", userEmail);
                    return uploadResult.Data.TempKey; // Thực tế là key thực
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