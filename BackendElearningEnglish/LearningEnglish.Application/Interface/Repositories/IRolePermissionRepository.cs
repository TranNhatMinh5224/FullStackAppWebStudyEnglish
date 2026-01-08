using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface;

public interface IRolePermissionRepository
{
    // Lấy tất cả permissions của user (qua roles)
    Task<List<RolePermission>> GetUserPermissionsAsync(int userId);
    
    // Lấy tất cả permissions của role
    Task<List<RolePermission>> GetRolePermissionsAsync(int roleId);
    
    // Gán permission cho role
    Task AssignPermissionToRoleAsync(int roleId, int permissionId);
    
    // Xóa permission khỏi role
    Task RemovePermissionFromRoleAsync(int roleId, int permissionId);
    
    // Xóa tất cả permissions của role
    Task RemoveAllPermissionsFromRoleAsync(int roleId);
    
    // Check user có permission không
    Task<bool> UserHasPermissionAsync(int userId, string permissionName);
    
    // Lưu thay đổi
    Task SaveChangesAsync();
}
