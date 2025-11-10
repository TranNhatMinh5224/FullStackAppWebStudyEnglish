using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using AutoMapper;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Application.Service
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public LoginService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IMapper mapper, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<ServiceResponse<AuthResponseDto>> LoginUserAsync(LoginUserDto dto)
        {
            var response = new ServiceResponse<AuthResponseDto>();
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (user == null || !user.VerifyPassword(dto.Password))
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Email hoặc mật khẩu không đúng";
                    return response;
                }
                if (user.Status == StatusAccount.Inactive)
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

                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "Đăng nhập thành công";
                response.Data = new AuthResponseDto { AccessToken = accessToken.Item1, RefreshToken = refreshToken.Token, ExpiresAt = accessToken.Item2, User = _mapper.Map<UserDto>(user) };
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
