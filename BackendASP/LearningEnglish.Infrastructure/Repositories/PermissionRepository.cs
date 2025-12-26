using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.PermissionId)
            .ToListAsync();
    }

    public async Task<Permission?> GetPermissionByIdAsync(int permissionId)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId);
    }

    public async Task<List<Permission>> GetPermissionsByIdsAsync(List<int> permissionIds)
    {
        return await _context.Permissions
            .Where(p => permissionIds.Contains(p.PermissionId))
            .ToListAsync();
    }

    public async Task<List<Permission>> GetPermissionsByCategoryAsync(string category)
    {
        return await _context.Permissions
            .Where(p => p.Category == category)
            .OrderBy(p => p.PermissionId)
            .ToListAsync();
    }

    public async Task<Permission?> GetPermissionByNameAsync(string permissionName)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName);
    }
}
