using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs.Admin;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Application.Service;

public class AdminManagementService : IAdminManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IMapper _mapper;

    public AdminManagementService(
        IUserRepository userRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<AdminDto>> CreateAdminAsync(CreateAdminDto dto)
    {
        var response = new ServiceResponse<AdminDto>();
        try
        {
            // Check email đã tồn tại
            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Email đã tồn tại";
                return response;
            }

            // Check permissions có hợp lệ không
            var permissions = await _permissionRepository.GetPermissionsByIdsAsync(dto.PermissionIds);
            if (permissions.Count != dto.PermissionIds.Count)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Một số permission không hợp lệ";
                return response;
            }

            // Tạo user mới
            var user = new User
            {
                Email = dto.Email,
                NormalizedEmail = dto.Email.ToUpper(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                EmailVerified = true,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Gán Admin role
            var adminRole = await _userRepository.GetRoleByNameAsync("Admin");
            if (adminRole == null)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Admin role không tồn tại trong hệ thống";
                return response;
            }

            user.Roles.Add(adminRole);
            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            // Gán permissions cho Admin role của user này
            // Note: Gán vào RolePermissions với RoleId = Admin role
            foreach (var permissionId in dto.PermissionIds)
            {
                await _rolePermissionRepository.AssignPermissionToRoleAsync(adminRole.RoleId, permissionId);
            }
            await _rolePermissionRepository.SaveChangesAsync();

            // Map response
            response.Data = new AdminDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                PhoneNumber = user.PhoneNumber,
                Roles = new List<string> { "Admin" },
                Permissions = _mapper.Map<List<PermissionDto>>(permissions),
                CreatedAt = user.CreatedAt,
                Status = user.Status.ToString()
            };

            response.StatusCode = 201;
            response.Message = "Tạo admin thành công";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<PagedResult<AdminDto>>> GetAdminsPagedAsync(AdminQueryParameters parameters)
    {
        var response = new ServiceResponse<PagedResult<AdminDto>>();
        try
        {
            // Lấy tất cả users có Admin role
            var adminsQuery = (await _userRepository.GetUsersByRoleAsync("Admin")).AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                adminsQuery = adminsQuery.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }

            var totalCount = adminsQuery.Count();

            // Pagination
            var admins = adminsQuery
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            // Map to DTOs with permissions
            var adminDtos = new List<AdminDto>();
            foreach (var admin in admins)
            {
                var rolePermissions = await _rolePermissionRepository.GetUserPermissionsAsync(admin.UserId);
                adminDtos.Add(new AdminDto
                {
                    UserId = admin.UserId,
                    Email = admin.Email,
                    FullName = $"{admin.FirstName} {admin.LastName}",
                    PhoneNumber = admin.PhoneNumber ?? string.Empty,
                    Roles = admin.Roles.Select(r => r.Name).ToList(),
                    Permissions = rolePermissions.Select(rp => _mapper.Map<PermissionDto>(rp.Permission)).ToList(),
                    CreatedAt = admin.CreatedAt,
                    Status = admin.Status.ToString()
                });
            }

            response.Data = new PagedResult<AdminDto>
            {
                Items = adminDtos,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };

            response.StatusCode = 200;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<UpdateAdminPermissionsResultDto>> UpdateAdminPermissionsAsync(UpdateAdminPermissionsDto dto)
    {
        var response = new ServiceResponse<UpdateAdminPermissionsResultDto>();
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy user";
                return response;
            }

            // Check user có Admin role không
            var adminRole = user.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole == null)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "User không phải admin";
                return response;
            }

            // Lấy permissions hiện tại
            var currentRolePermissions = await _rolePermissionRepository.GetRolePermissionsAsync(adminRole.RoleId);
            var currentPermissionIds = currentRolePermissions.Select(rp => rp.PermissionId).ToList();

            // Tính toán removed và added
            var removedIds = currentPermissionIds.Except(dto.PermissionIds).ToList();
            var addedIds = dto.PermissionIds.Except(currentPermissionIds).ToList();

            // Remove old permissions
            foreach (var permissionId in removedIds)
            {
                await _rolePermissionRepository.RemovePermissionFromRoleAsync(adminRole.RoleId, permissionId);
            }

            // Add new permissions
            foreach (var permissionId in addedIds)
            {
                await _rolePermissionRepository.AssignPermissionToRoleAsync(adminRole.RoleId, permissionId);
            }

            await _rolePermissionRepository.SaveChangesAsync();

            // Get permission names
            var removedPermissions = await _permissionRepository.GetPermissionsByIdsAsync(removedIds);
            var addedPermissions = await _permissionRepository.GetPermissionsByIdsAsync(addedIds);
            var currentPermissions = await _permissionRepository.GetPermissionsByIdsAsync(dto.PermissionIds);

            response.Data = new UpdateAdminPermissionsResultDto
            {
                UserId = user.UserId,
                Email = user.Email,
                RemovedPermissions = removedPermissions.Select(p => p.Name).ToList(),
                AddedPermissions = addedPermissions.Select(p => p.Name).ToList(),
                CurrentPermissions = _mapper.Map<List<PermissionDto>>(currentPermissions)
            };

            response.StatusCode = 200;
            response.Message = "Cập nhật permissions thành công";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<RoleOperationResultDto>> DeleteAdminAsync(int userId)
    {
        var response = new ServiceResponse<RoleOperationResultDto>();
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy user";
                return response;
            }

            var adminRole = user.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole == null)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "User không phải admin";
                return response;
            }

            // Remove Admin role
            user.Roles.Remove(adminRole);

            // Remove all permissions của Admin role
            await _rolePermissionRepository.RemoveAllPermissionsFromRoleAsync(adminRole.RoleId);

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();
            await _rolePermissionRepository.SaveChangesAsync();

            response.Data = new RoleOperationResultDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };

            response.StatusCode = 200;
            response.Message = "Đã xóa admin và thu hồi tất cả permissions";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<bool>> ResetAdminPasswordAsync(ResetAdminPasswordDto dto)
    {
        var response = new ServiceResponse<bool>();
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy user";
                return response;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            response.Data = true;
            response.StatusCode = 200;
            response.Message = "Reset password thành công";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<bool>> ChangeAdminEmailAsync(ChangeAdminEmailDto dto)
    {
        var response = new ServiceResponse<bool>();
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy user";
                return response;
            }

            // Check email mới đã tồn tại chưa
            var existingUser = await _userRepository.GetUserByEmailAsync(dto.NewEmail);
            if (existingUser != null && existingUser.UserId != dto.UserId)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Email đã tồn tại";
                return response;
            }

            user.Email = dto.NewEmail;
            user.NormalizedEmail = dto.NewEmail.ToUpper();
            user.EmailVerified = false; // Cần verify lại
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            response.Data = true;
            response.StatusCode = 200;
            response.Message = "Đổi email thành công";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<RoleOperationResultDto>> AssignRoleAsync(AssignRoleDto dto)
    {
        var response = new ServiceResponse<RoleOperationResultDto>();
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy user";
                return response;
            }

            var role = await _userRepository.GetRoleByNameAsync(dto.RoleName);
            if (role == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = $"Role '{dto.RoleName}' không tồn tại";
                return response;
            }

            // Check đã có role này chưa
            if (user.Roles.Any(r => r.RoleId == role.RoleId))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = $"User đã có role '{dto.RoleName}'";
                return response;
            }

            user.Roles.Add(role);
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            response.Data = new RoleOperationResultDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };

            response.StatusCode = 200;
            response.Message = $"Gán role '{dto.RoleName}' thành công";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<RoleOperationResultDto>> RemoveRoleAsync(RemoveRoleDto dto)
    {
        var response = new ServiceResponse<RoleOperationResultDto>();
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy user";
                return response;
            }

            var role = user.Roles.FirstOrDefault(r => r.Name == dto.RoleName);
            if (role == null)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = $"User không có role '{dto.RoleName}'";
                return response;
            }

            user.Roles.Remove(role);
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            response.Data = new RoleOperationResultDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };

            response.StatusCode = 200;
            response.Message = $"Xóa role '{dto.RoleName}' thành công";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }
}
