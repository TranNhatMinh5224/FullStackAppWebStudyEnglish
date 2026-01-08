using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs.Admin;

namespace LearningEnglish.Application.Interface.AdminManagement
{

public interface IPermissionService
{
    
    /// Lấy tất cả permissions
 
    Task<ServiceResponse<List<PermissionDto>>> GetAllPermissionsAsync();
    
 
    /// Lấy permissions của user
    Task<ServiceResponse<UserPermissionsDto>> GetUserPermissionsAsync(int userId);
}
}
