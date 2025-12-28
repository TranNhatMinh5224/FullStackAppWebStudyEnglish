using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Constants;

namespace LearningEnglish.API.Authorization
{
   
   
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            IRolePermissionRepository rolePermissionRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PermissionAuthorizationHandler> logger)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("User chưa authenticated");
                return;
            }

            var roles = context.User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            _logger.LogInformation(" Checking permissions. User roles: {Roles}, Required permissions: {Permissions}", 
                string.Join(", ", roles), string.Join(", ", requirement.Permissions));

            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 1: KIỂM TRA SUPERADMIN (TỰ ĐỘNG CÓ TẤT CẢ QUYỀN)
            // ═══════════════════════════════════════════════════════════════
            if (roles.Any(r => RoleConstants.IsSuperAdmin(r)))
            {
                _logger.LogInformation(" SuperAdmin tự động có quyền truy cập (toàn quyền)");
                context.Succeed(requirement);
                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 2: KIỂM TRA PERMISSION (KHÔNG GIỚI HẠN THEO ROLE)
            // ═══════════════════════════════════════════════════════════════
            // REFACTOR: Bỏ check isAdmin để cho phép mở rộng permission cho các role khác
            // (ví dụ: Teacher VIP, Moderator có thể có permission đặc biệt)
            // Logic: Chỉ cần user có permission trong DB là được, không cần phải là Admin
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning(" Không tìm thấy userId trong claims. Claims: {Claims}", 
                    string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}")));
                return;
            }

            _logger.LogInformation("Checking permissions for UserId: {UserId} (Roles: {Roles})", 
                userId, string.Join(", ", roles));

            // Kiểm tra từng permission (OR logic - có 1 trong các permissions là đủ)
            foreach (var permissionName in requirement.Permissions)
            {
                var hasPermission = await _rolePermissionRepository.UserHasPermissionAsync(userId, permissionName);
                
                _logger.LogInformation(" User {UserId} - Permission '{Permission}': {HasPermission}", 
                    userId, permissionName, hasPermission ? "✅ CÓ" : "❌ KHÔNG CÓ");
                
                if (hasPermission)
                {
                    _logger.LogInformation(" User {UserId} có permission {Permission} - Cho phép truy cập", userId, permissionName);
                    context.Succeed(requirement);
                    return;
                }
            }

            _logger.LogWarning("User {UserId} KHÔNG CÓ permission: {Permissions} - Từ chối truy cập", 
                userId, string.Join(", ", requirement.Permissions));
        }
    }

   
    //Requirement cho permission authorization
   
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public List<string> Permissions { get; }

        public PermissionRequirement(params string[] permissions)
        {
            Permissions = permissions.ToList();
        }
    }
}

