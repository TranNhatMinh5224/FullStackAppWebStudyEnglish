using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using CleanDemo.Domain.Enums; 
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
        // Implement phương thức block account
        public async Task<ServiceResponse<BlockAccountResponseDto>> BlockAccountAsync(int userId)
        {
            var response = new ServiceResponse<BlockAccountResponseDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy tài khoản người dùng";
                    return response;
                }

                var isAdmin = await _userRepository.GetUserRolesAsync(userId);
                if (isAdmin)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không thể block tài khoản Admin";
                    return response;
                }

                if (user.Status == StatusAccount.Inactive)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Tài khoản đã bị khóa trước đó";
                    return response;
                }
                user.Status = StatusAccount.Inactive;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Success = true;
                response.Data = new BlockAccountResponseDto
                {
                    Message = "Block tài khoản thành công"
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
        // Implement cho phương thức unblock account
        public async Task<ServiceResponse<UnblockAccountResponseDto>> UnblockAccountAsync(int userId)
        {
            var response = new ServiceResponse<UnblockAccountResponseDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy tài khoản người dùng";
                    return response;
                }

                if (user.Status == StatusAccount.Active)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Tài khoản hiện không bị khóa";
                    return response;
                }

                user.Status = StatusAccount.Active;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Success = true;
                response.Data = new UnblockAccountResponseDto
                {
                    Message = "Unblock tài khoản thành công"
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
        // Implement cho phương thức lấy danh sách tài khoản bị khóa
        public async Task<ServiceResponse<List<UserDto>>> GetListBlockedAccountsAsync()
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                var ListBlockedUsers = users.Where(u => u.Status == StatusAccount.Inactive).ToList();
                response.StatusCode = 200;
                response.Success = true;
                response.Data = _mapper.Map<List<UserDto>>(ListBlockedUsers);
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
