using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs.Admin;

namespace LearningEnglish.Application.Interface.AdminManagement;

public interface IAdminManagementService
{
    
    // Tạo admin mới (tạo user + gán Admin role + gán permissions)
    
    Task<ServiceResponse<AdminDto>> CreateAdminAsync(CreateAdminDto dto);
    
   
    // Lấy danh sách admins với phân trang
   
    Task<ServiceResponse<PagedResult<AdminDto>>> GetAdminsPagedAsync(AdminQueryParameters parameters);
    
   
    //Lấy admin theo userId
  
    Task<ServiceResponse<AdminDto>> GetAdminByIdAsync(int userId);
    
 
    /// Xóa admin (remove Admin role)
    
    Task<ServiceResponse<RoleOperationResultDto>> DeleteAdminAsync(int userId);
    

    // Reset password admin

    Task<ServiceResponse<bool>> ResetAdminPasswordAsync(ResetAdminPasswordDto dto);
    

    // Đổi email admin
 
    Task<ServiceResponse<bool>> ChangeAdminEmailAsync(ChangeAdminEmailDto dto);
    
    //Gán role cho user

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
