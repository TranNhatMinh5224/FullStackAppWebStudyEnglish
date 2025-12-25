using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface;

public interface IPermissionRepository
{
    // Lấy tất cả permissions
    Task<List<Permission>> GetAllPermissionsAsync();
    
    // Lấy permission theo ID
    Task<Permission?> GetPermissionByIdAsync(int permissionId);
    
    // Lấy permissions theo IDs
    Task<List<Permission>> GetPermissionsByIdsAsync(List<int> permissionIds);
    
    // Lấy permissions theo category
    Task<List<Permission>> GetPermissionsByCategoryAsync(string category);
}
