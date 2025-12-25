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
    /// Update permissions của admin (replace toàn bộ)
    /// </summary>
    Task<ServiceResponse<UpdateAdminPermissionsResultDto>> UpdateAdminPermissionsAsync(UpdateAdminPermissionsDto dto);
    
    /// <summary>
    /// Xóa admin (remove Admin role + remove permissions)
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
}
