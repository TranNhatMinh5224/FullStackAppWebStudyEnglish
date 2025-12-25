using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs.Admin;

namespace LearningEnglish.Application.Interface;

public interface IPermissionService
{
    /// <summary>
    /// Lấy tất cả permissions
    /// </summary>
    Task<ServiceResponse<List<PermissionDto>>> GetAllPermissionsAsync();
    
    /// <summary>
    /// Lấy permissions của user
    /// </summary>
    Task<ServiceResponse<UserPermissionsDto>> GetUserPermissionsAsync(int userId);
}
