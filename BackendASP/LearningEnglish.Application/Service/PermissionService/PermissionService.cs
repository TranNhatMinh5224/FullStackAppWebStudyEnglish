using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs.Admin;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.AdminManagement;

namespace LearningEnglish.Application.Service;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public PermissionService(
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<List<PermissionDto>>> GetAllPermissionsAsync()
    {
        var response = new ServiceResponse<List<PermissionDto>>();
        try
        {
            var permissions = await _permissionRepository.GetAllPermissionsAsync();
            response.Data = _mapper.Map<List<PermissionDto>>(permissions);
            response.StatusCode = 200;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<UserPermissionsDto>> GetUserPermissionsAsync(int userId)
    {
        var response = new ServiceResponse<UserPermissionsDto>();
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy user";
                return response;
            }

            var rolePermissions = await _rolePermissionRepository.GetUserPermissionsAsync(userId);

            response.Data = new UserPermissionsDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = user.Roles.Select(r => r.Name).ToList(),
                Permissions = rolePermissions.Select(rp => new PermissionWithAssignmentDto
                {
                    PermissionId = rp.Permission.PermissionId,
                    Name = rp.Permission.Name,
                    DisplayName = rp.Permission.DisplayName,
                    Category = rp.Permission.Category,
                    AssignedAt = rp.AssignedAt
                }).ToList()
            };

            response.StatusCode = 200;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Lỗi hệ thống: {ex.Message}";
        }
        return response;
    }
}
