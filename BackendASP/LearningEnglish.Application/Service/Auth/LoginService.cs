using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface.Auth;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Helpers;
using AutoMapper;
using LearningEnglish.Domain.Enums;


namespace LearningEnglish.Application.Service
{
    // Service xử lý đăng nhập bằng email và mật khẩu
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private const string AvatarBucket = "avatars";

        // Constructor khởi tạo các dependency injection
        public LoginService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IMapper mapper, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        // Xử lý đăng nhập bằng email và mật khẩu
        public async Task<ServiceResponse<AuthResponseDto>> LoginUserAsync(LoginUserDto dto)
        {
            var response = new ServiceResponse<AuthResponseDto>();
            try
            {
                // Đăng nhập bằng email
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);

                // Kiểm tra người dùng tồn tại và mật khẩu đúng
                if (user == null || !user.VerifyPassword(dto.Password))
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Email hoặc mật khẩu không đúng";
                    return response;
                }

                // Kiểm tra email đã được xác thực chưa
                if (!user.EmailVerified)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Vui lòng xác thực email trước khi đăng nhập. Kiểm tra hộp thư của bạn để lấy mã OTP.";
                    return response;
                }

                // Kiểm tra trạng thái tài khoản
                if (user.Status == AccountStatus.Inactive)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Tài khoản của bạn đã bị khóa do vi phạm chính sách!";
                    response.Data = new AuthResponseDto
                    {
                        User = _mapper.Map<UserDto>(user),
                    };
                    return response;
                }

                // Tạo JWT access token và refresh token
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "Đăng nhập thành công";
                
                var userDto = _mapper.Map<UserDto>(user);
                
                // Tạo URL công khai cho avatar nếu có
                if (!string.IsNullOrWhiteSpace(user.AvatarKey))
                {
                    userDto.AvatarUrl = BuildPublicUrl.BuildURL(AvatarBucket, user.AvatarKey);
                }
                
                response.Data = new AuthResponseDto 
                { 
                    AccessToken = accessToken.Item1, 
                    RefreshToken = refreshToken.Token, 
                    ExpiresAt = accessToken.Item2, 
                    User = userDto 
                };
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }
    }

}
