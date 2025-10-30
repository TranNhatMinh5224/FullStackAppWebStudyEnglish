using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using AutoMapper;

namespace CleanDemo.Application.Service
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserManagementService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<UserDto>> GetUserProfileAsync(int userId)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                response.StatusCode = 200;
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

        public async Task<ServiceResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserDto dto)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                _mapper.Map(dto, user);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "Cập nhật hồ sơ thành công";
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

        public async Task<ServiceResponse<List<UserDto>>> GetAllUsersAsync()
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                response.StatusCode = 200;
                response.Data = _mapper.Map<List<UserDto>>(users);
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
