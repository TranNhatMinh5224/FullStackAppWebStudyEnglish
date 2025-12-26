using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
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
    private readonly IRoleRepository _roleRepository;
    private readonly ITeacherSubscriptionService _teacherSubscriptionService;
    private readonly ITeacherPackageRepository _teacherPackageRepository;
    private readonly IMapper _mapper;

    public AdminManagementService(
        IUserRepository userRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IRoleRepository roleRepository,
        ITeacherSubscriptionService teacherSubscriptionService,
        ITeacherPackageRepository teacherPackageRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _roleRepository = roleRepository;
        _teacherSubscriptionService = teacherSubscriptionService;
        _teacherPackageRepository = teacherPackageRepository;
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

            // Validate role ID - chỉ chấp nhận ContentAdmin (2) hoặc FinanceAdmin (3)
            var validRoleIds = new[] { 2, 3 }; // ContentAdmin = 2, FinanceAdmin = 3
            if (!validRoleIds.Contains(dto.RoleId))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = $"RoleId không hợp lệ. Chỉ chấp nhận: 2 (ContentAdmin) hoặc 3 (FinanceAdmin)";
                return response;
            }

            // Lấy role theo ID
            var adminRole = await _roleRepository.GetRoleByIdAsync(dto.RoleId);
            if (adminRole == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = $"Role với RoleId {dto.RoleId} không tồn tại trong hệ thống";
                return response;
            }

            // Validate role name phải là ContentAdmin hoặc FinanceAdmin
            var validRoleNames = new[] { "ContentAdmin", "FinanceAdmin" };
            if (!validRoleNames.Contains(adminRole.Name, StringComparer.OrdinalIgnoreCase))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = $"RoleId {dto.RoleId} không phải là ContentAdmin hoặc FinanceAdmin";
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

            user.Roles.Add(adminRole);
            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();
            
            // Permissions đã được gán cho role trong seeder, không cần gán thêm

            // Lấy permissions từ role
            var rolePermissions = await _rolePermissionRepository.GetRolePermissionsAsync(adminRole.RoleId);
            var permissionDtos = rolePermissions.Select(rp => _mapper.Map<PermissionDto>(rp.Permission)).ToList();

            // Map response
            response.Data = new AdminDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                PhoneNumber = user.PhoneNumber,
                Roles = new List<string> { adminRole.Name },
                Permissions = permissionDtos,
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
            // Lấy tất cả users có Admin roles (ContentAdmin, FinanceAdmin, SuperAdmin)
            var contentAdmins = await _userRepository.GetUsersByRoleAsync("ContentAdmin");
            var financeAdmins = await _userRepository.GetUsersByRoleAsync("FinanceAdmin");
            var superAdmins = await _userRepository.GetUsersByRoleAsync("SuperAdmin");
            
            // Combine tất cả admin users
            var allAdmins = contentAdmins
                .Union(financeAdmins)
                .Union(superAdmins)
                .DistinctBy(u => u.UserId)
                .ToList();
            
            var adminsQuery = allAdmins.AsQueryable();

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
                // Lấy permissions từ role của admin
                var adminRole = admin.Roles.FirstOrDefault(r => r.Name == "ContentAdmin" || r.Name == "FinanceAdmin" || r.Name == "SuperAdmin");
                var permissions = new List<Permission>();
                
                if (adminRole != null)
                {
                    var rolePermissions = await _rolePermissionRepository.GetRolePermissionsAsync(adminRole.RoleId);
                    permissions = rolePermissions.Select(rp => rp.Permission).ToList();
                }
                
                adminDtos.Add(new AdminDto
                {
                    UserId = admin.UserId,
                    Email = admin.Email,
                    FullName = $"{admin.FirstName} {admin.LastName}",
                    PhoneNumber = admin.PhoneNumber ?? string.Empty,
                    Roles = admin.Roles.Select(r => r.Name).ToList(),
                    Permissions = permissions.Select(p => _mapper.Map<PermissionDto>(p)).ToList(),
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

    public async Task<ServiceResponse<AdminDto>> GetAdminByIdAsync(int userId)
    {
        var response = new ServiceResponse<AdminDto>();
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

            // Kiểm tra user có phải admin không (ContentAdmin, FinanceAdmin, SuperAdmin)
            var adminRole = user.Roles.FirstOrDefault(r => 
                r.Name == "ContentAdmin" || r.Name == "FinanceAdmin" || r.Name == "SuperAdmin");
            
            if (adminRole == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "User không phải admin";
                return response;
            }

            // Lấy permissions từ role
            var rolePermissions = await _rolePermissionRepository.GetRolePermissionsAsync(adminRole.RoleId);
            var permissionDtos = rolePermissions.Select(rp => _mapper.Map<PermissionDto>(rp.Permission)).ToList();

            response.Data = new AdminDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Roles = user.Roles.Select(r => r.Name).ToList(),
                Permissions = permissionDtos,
                CreatedAt = user.CreatedAt,
                Status = user.Status.ToString()
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

            var adminRole = user.Roles.FirstOrDefault(r => 
                r.Name == "ContentAdmin" || r.Name == "FinanceAdmin" || r.Name == "SuperAdmin");
            if (adminRole == null)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "User không phải admin";
                return response;
            }

            // Remove Admin role
            // Không cần remove permissions vì permissions được gán cho role, không phải cho user
            user.Roles.Remove(adminRole);

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

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

    public async Task<ServiceResponse<RoleOperationResultDto>> UpgradeUserToTeacherAsync(UpgradeUserToTeacherDto dto)
    {
        var response = new ServiceResponse<RoleOperationResultDto>();
        try
        {
            // 1. Tìm user theo email
            var user = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = $"Không tìm thấy user với email: {dto.Email}";
                return response;
            }

            // 2. Kiểm tra user đã có role Teacher chưa
            if (user.Roles.Any(r => r.Name.Equals("Teacher", StringComparison.OrdinalIgnoreCase)))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = $"User {dto.Email} đã có role Teacher";
                return response;
            }

            // 3. Kiểm tra TeacherPackage có tồn tại không
            var package = await _teacherPackageRepository.GetTeacherPackageByIdAsync(dto.TeacherPackageId);
            if (package == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = $"Không tìm thấy TeacherPackage với ID: {dto.TeacherPackageId}";
                return response;
            }

            // 4. Gán role Teacher cho user
            var roleUpdated = await _userRepository.UpdateRoleTeacher(user.UserId);
            if (!roleUpdated)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Không thể nâng cấp tài khoản lên giáo viên";
                return response;
            }

            // 5. Tạo TeacherSubscription (thay thế cho việc mua gói thất bại)
            var subscriptionDto = new PurchaseTeacherPackageDto
            {
                IdTeacherPackage = dto.TeacherPackageId
            };
            var subscriptionResult = await _teacherSubscriptionService.AddTeacherSubscriptionAsync(subscriptionDto, user.UserId);
            if (!subscriptionResult.Success)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Tạo subscription thất bại: {subscriptionResult.Message}";
                return response;
            }

            // 6. Lưu tất cả thay đổi (Role + Subscription)
            await _userRepository.SaveChangesAsync();

            // 7. Reload user để lấy roles mới nhất
            user = await _userRepository.GetByIdAsync(user.UserId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi reload user sau khi nâng cấp";
                return response;
            }

            response.Data = new RoleOperationResultDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };

            response.StatusCode = 200;
            response.Message = $"Đã nâng cấp user {dto.Email} thành Teacher với gói {package.PackageName} thành công";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    // ═══════════════════════════════════════════════════════════════
    // ROLE MANAGEMENT - Chỉ SuperAdmin
    // ═══════════════════════════════════════════════════════════════

    public async Task<ServiceResponse<List<RoleDto>>> GetAllRolesAsync()
    {
        var response = new ServiceResponse<List<RoleDto>>();
        try
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            
            var roleDtos = new List<RoleDto>();
            foreach (var role in roles)
            {
                var rolePermissions = await _rolePermissionRepository.GetRolePermissionsAsync(role.RoleId);
                var userCount = role.Users?.Count ?? 0;
                
                roleDtos.Add(new RoleDto
                {
                    RoleId = role.RoleId,
                    Name = role.Name,
                    Permissions = rolePermissions.Select(rp => _mapper.Map<PermissionDto>(rp.Permission)).ToList(),
                    UserCount = userCount
                });
            }

            response.Data = roleDtos;
            response.Success = true;
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

    // ═══════════════════════════════════════════════════════════════
    // PERMISSION MANAGEMENT - Chỉ SuperAdmin (Read-only)
    // ═══════════════════════════════════════════════════════════════

    public async Task<ServiceResponse<List<PermissionDto>>> GetAllPermissionsAsync()
    {
        var response = new ServiceResponse<List<PermissionDto>>();
        try
        {
            var permissions = await _permissionRepository.GetAllPermissionsAsync();
            response.Data = permissions.Select(p => _mapper.Map<PermissionDto>(p)).ToList();
            response.Success = true;
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
}
