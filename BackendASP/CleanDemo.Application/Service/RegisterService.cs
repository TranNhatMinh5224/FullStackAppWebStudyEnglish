using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using AutoMapper;


namespace CleanDemo.Application.Service
{
    public class RegisterService : IRegisterService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RegisterService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Email đã tồn tại trong hệ thống";
                    return response;
                }

                var user = _mapper.Map<User>(dto);
                user.SetPassword(dto.Password);

                
                var studentRole = await _userRepository.GetRoleByNameAsync("Student");
                if (studentRole == null)
                {
                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Không tìm thấy vai trò mặc định 'Student'";
                    return response;
                }

                user.Roles = new List<Role> { studentRole };

                await _userRepository.AddUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 201;
                response.Message = "Đăng ký tài khoản thành công";
                response.Data = _mapper.Map<UserDto>(user);
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
