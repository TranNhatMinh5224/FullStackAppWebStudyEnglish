using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.AdminManagement;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using AutoMapper;
using Microsoft.Extensions.Logging;
using LearningEnglish.Domain.Enums;


namespace LearningEnglish.Application.Service
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserManagementService> _logger;
        private readonly IAvatarService _avatarService;
        public UserManagementService(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UserManagementService> logger,
            IAvatarService avatarService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _avatarService = avatarService;
        }




        // Lấy tất cả người dùng với phân trang, search và sort

        public async Task<ServiceResponse<PagedResult<UserDto>>> GetAllUsersPagedAsync(UserQueryParameters request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                // Directly pass UserQueryParameters to repository
                var pagedData = await _userRepository.GetAllUsersPagedAsync(request);

                var userDtos = _mapper.Map<List<UserDto>>(pagedData.Items);

                // Build AvatarUrl từ AvatarUrl (đã là key từ mapping) cho tất cả users (giống pattern GetSystemCoursesAsync)
                foreach (var userDto in userDtos)
                {
                    if (!string.IsNullOrWhiteSpace(userDto.AvatarUrl))
                    {
                        userDto.AvatarUrl = _avatarService.BuildAvatarUrl(userDto.AvatarUrl);
                    }
                }

                var result = new PagedResult<UserDto>
                {
                    Items = userDtos,
                    TotalCount = pagedData.TotalCount,
                    PageNumber = pagedData.PageNumber,
                    PageSize = pagedData.PageSize
                };

                response.StatusCode = 200;
                response.Success = true;
                response.Data = result;
                _logger.LogInformation("Admin retrieved {Count} users on page {Page}", result.Items.Count, request.PageNumber);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi: {ex.Message}";
                _logger.LogError(ex, "Error in GetAllUsersPagedAsync with page {Page}, size {Size}", request.PageNumber, request.PageSize);
            }
            return response;
        }

        // Block account
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

                if (user.Status == AccountStatus.Inactive)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Tài khoản đã bị khóa trước đó";
                    return response;
                }

                user.Status = AccountStatus.Inactive;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Success = true;
                response.Data = new BlockAccountResponseDto { Message = "Block tài khoản thành công" };
                _logger.LogInformation("Admin blocked user account {UserId}", userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";                _logger.LogError(ex, "Error blocking user account {UserId}", userId);            }
            return response;
        }

        // Unblock account
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

                if (user.Status == AccountStatus.Active)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Tài khoản hiện không bị khóa";
                    return response;
                }

                user.Status = AccountStatus.Active;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Success = true;
                response.Data = new UnblockAccountResponseDto { Message = "Unblock tài khoản thành công" };
                _logger.LogInformation("Admin unblocked user account {UserId}", userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";                _logger.LogError(ex, "Error unblocking user account {UserId}", userId);            }
            return response;
        }

        // Danh sách tài khoản bị khóa với phân trang
        public async Task<ServiceResponse<PagedResult<UserDto>>> GetListBlockedAccountsPagedAsync(UserQueryParameters request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                // Directly pass UserQueryParameters to repository
                var pagedData = await _userRepository.GetListBlockedAccountsPagedAsync(request);

                var userDtos = _mapper.Map<List<UserDto>>(pagedData.Items);

                // Build AvatarUrl từ AvatarUrl (đã là key từ mapping) cho tất cả users (giống pattern GetSystemCoursesAsync)
                foreach (var userDto in userDtos)
                {
                    if (!string.IsNullOrWhiteSpace(userDto.AvatarUrl))
                    {
                        userDto.AvatarUrl = _avatarService.BuildAvatarUrl(userDto.AvatarUrl);
                    }
                }

                var result = new PagedResult<UserDto>
                {
                    Items = userDtos,
                    TotalCount = pagedData.TotalCount,
                    PageNumber = pagedData.PageNumber,
                    PageSize = pagedData.PageSize
                };

                response.StatusCode = 200;
                response.Success = true;
                response.Data = result;
                _logger.LogInformation("Admin retrieved {Count} blocked accounts on page {Page}", result.Items.Count, request.PageNumber);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in GetListBlockedAccountsPagedAsync with page {Page}, size {Size}", request.PageNumber, request.PageSize);
            }
            return response;
        }

        

        // Lấy danh sách giáo viên theo phân trang
        public async Task<ServiceResponse<PagedResult<UserDto>>> GetListTeachersPagedAsync(UserQueryParameters request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                
                var pagedData = await _userRepository.GetAllTeachersPagedAsync(request);

                var userDtos = _mapper.Map<List<UserDto>>(pagedData.Items);

                // Build AvatarUrl từ AvatarUrl (đã là key từ mapping) cho tất cả users (giống pattern GetSystemCoursesAsync)
                foreach (var userDto in userDtos)
                {
                    if (!string.IsNullOrWhiteSpace(userDto.AvatarUrl))
                    {
                        userDto.AvatarUrl = _avatarService.BuildAvatarUrl(userDto.AvatarUrl);
                    }
                }

                var result = new PagedResult<UserDto>
                {
                    Items = userDtos,
                    TotalCount = pagedData.TotalCount,
                    PageNumber = pagedData.PageNumber,
                    PageSize = pagedData.PageSize
                };

                response.StatusCode = 200;
                response.Success = true;
                response.Data = result;
                _logger.LogInformation("Admin retrieved {Count} teachers on page {Page}", result.Items.Count, request.PageNumber);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in GetListTeachersPagedAsync with page {Page}, size {Size}", request.PageNumber, request.PageSize);
            }
            return response;
        }

        // Lấy chi tiết user theo ID
        public async Task<ServiceResponse<UserDto>> GetUserByIdAsync(int userId)
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

                // Build URL cho avatar nếu tồn tại
                if (!string.IsNullOrWhiteSpace(user.AvatarKey))
                {
                    response.Data.AvatarUrl = _avatarService.BuildAvatarUrl(user.AvatarKey);
                    _logger.LogInformation("Built avatar URL for user {UserId}: {AvatarUrl}", userId, response.Data.AvatarUrl);
                }
                else
                {
                    _logger.LogInformation("User {UserId} does not have AvatarKey", userId);
                    // Đảm bảo AvatarUrl là null hoặc empty nếu không có AvatarKey
                    response.Data.AvatarUrl = null;
                }

                response.Success = true;
                _logger.LogInformation("Admin retrieved user detail for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error retrieving user detail for user {UserId}", userId);
            }
            return response;
        }
    }
}
