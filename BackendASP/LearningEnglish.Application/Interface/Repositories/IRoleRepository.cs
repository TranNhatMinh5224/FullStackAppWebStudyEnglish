using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface;

public interface IRoleRepository
{
    // Lấy tất cả roles
    Task<List<Role>> GetAllRolesAsync();
    
    // Lấy role theo ID
    Task<Role?> GetRoleByIdAsync(int roleId);
    
    // Lấy role theo tên
    Task<Role?> GetRoleByNameAsync(string roleName);
}

