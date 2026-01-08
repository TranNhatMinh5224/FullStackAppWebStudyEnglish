using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly AppDbContext _context;

    public RolePermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RolePermission>> GetUserPermissionsAsync(int userId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => rp.Role.Users.Any(u => u.UserId == userId))
            .ToListAsync();
    }

    public async Task<List<RolePermission>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
    }

    public async Task AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        var exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (!exists)
        {
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                AssignedAt = DateTime.UtcNow
            };
            await _context.RolePermissions.AddAsync(rolePermission);
        }
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
        }
    }

    public async Task RemoveAllPermissionsFromRoleAsync(int roleId)
    {
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(rolePermissions);
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permissionName)
    {
        // Check permissions từ role của user
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .AnyAsync(rp => rp.Permission.Name == permissionName && 
                           rp.Role.Users.Any(u => u.UserId == userId));
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
