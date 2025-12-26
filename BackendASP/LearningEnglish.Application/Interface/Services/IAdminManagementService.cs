using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs.Admin;

namespace LearningEnglish.Application.Interface;

public interface IAdminManagementService
{
    /// <summary>
    /// Tạo admin mới (tạo user + gán Admin role + gán permissions)
    /// </summary>
    Task<ServiceResponse<AdminDto>> CreateAdminAsync(CreateAdminDto dto);
    
    /// <summary>
    /// Lấy danh sách admins với phân trang
    /// </summary>
    Task<ServiceResponse<PagedResult<AdminDto>>> GetAdminsPagedAsync(AdminQueryParameters parameters);
    
    /// <summary>
    /// Lấy admin theo userId
    /// </summary>
    Task<ServiceResponse<AdminDto>> GetAdminByIdAsync(int userId);
    
    /// <summary>
    /// Xóa admin (remove Admin role)
    /// </summary>
    Task<ServiceResponse<RoleOperationResultDto>> DeleteAdminAsync(int userId);
    
    /// <summary>
    /// Reset password admin
    /// </summary>
    Task<ServiceResponse<bool>> ResetAdminPasswordAsync(ResetAdminPasswordDto dto);
    
    /// <summary>
    /// Đổi email admin
    /// </summary>
    Task<ServiceResponse<bool>> ChangeAdminEmailAsync(ChangeAdminEmailDto dto);
    
    /// <summary>
    /// Gán role cho user
    /// </summary>
    Task<ServiceResponse<RoleOperationResultDto>> AssignRoleAsync(AssignRoleDto dto);
    
    /// <summary>
    /// Xóa role khỏi user
    /// </summary>
    Task<ServiceResponse<RoleOperationResultDto>> RemoveRoleAsync(RemoveRoleDto dto);

    /// <summary>
    /// Nâng cấp user thành Teacher (gán role + tạo subscription)
    /// Dùng khi thanh toán thất bại hoặc cần xử lý thủ công
    /// </summary>
    Task<ServiceResponse<RoleOperationResultDto>> UpgradeUserToTeacherAsync(UpgradeUserToTeacherDto dto);

    // ═══════════════════════════════════════════════════════════════
    // ROLE & PERMISSION VIEW - Chỉ SuperAdmin (Read-only, fix cứng)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Lấy danh sách tất cả roles (read-only, fix cứng trong seed data)
    /// </summary>
    Task<ServiceResponse<List<RoleDto>>> GetAllRolesAsync();
    
    /// <summary>
    /// Lấy danh sách tất cả permissions (read-only, fix cứng trong seed data)
    /// </summary>
    Task<ServiceResponse<List<PermissionDto>>> GetAllPermissionsAsync();
}
