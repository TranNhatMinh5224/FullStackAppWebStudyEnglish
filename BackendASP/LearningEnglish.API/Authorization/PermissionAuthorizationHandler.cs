using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using LearningEnglish.Application.Interface;

namespace LearningEnglish.API.Authorization
{
   
    //Authorization handler để kiểm tra permission của Admin
    // Logic:
    // - SuperAdmin: Tự động pass (toàn quyền, không cần check permission)
    // - Content Admin: Chỉ có permissions 1,2,3 (Course, Lesson, Content)
    // - Finance Admin: Chỉ có permissions 4,5,6,7 (User, Payment, Revenue, Package)
   
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

            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 1: KIỂM TRA SUPERADMIN - TỰ ĐỘNG PASS
            // ═══════════════════════════════════════════════════════════════
            var roles = context.User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            _logger.LogInformation(" Checking permissions. User roles: {Roles}, Required permissions: {Permissions}", 
                string.Join(", ", roles), string.Join(", ", requirement.Permissions));

            if (roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation(" SuperAdmin tự động có quyền truy cập (toàn quyền)");
                context.Succeed(requirement);
                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 2: KIỂM TRA ADMIN ROLE
            // ═══════════════════════════════════════════════════════════════
            // Nếu không phải Admin, không cho phép
            // LƯU Ý: [RequirePermission] chỉ dành cho Admin endpoints
            // Teacher không được phép truy cập Admin endpoints có [RequirePermission]
            if (!roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning(" User không phải Admin hoặc SuperAdmin. Roles: {Roles}. [RequirePermission] chỉ dành cho Admin endpoints", 
                    string.Join(", ", roles));
                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 3: KIỂM TRA PERMISSION CỦA ADMIN
            // ═══════════════════════════════════════════════════════════════
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning(" Không tìm thấy userId trong claims. Claims: {Claims}", 
                    string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}")));
                return;
            }

            _logger.LogInformation("Checking permissions for Admin UserId: {UserId}", userId);

            // Kiểm tra từng permission (OR logic - có 1 trong các permissions là đủ)
            foreach (var permissionName in requirement.Permissions)
            {
                var hasPermission = await _rolePermissionRepository.UserHasPermissionAsync(userId, permissionName);
                
                _logger.LogInformation(" Admin {UserId} - Permission '{Permission}': {HasPermission}", 
                    userId, permissionName, hasPermission ? "✅ CÓ" : "❌ KHÔNG CÓ");
                
                if (hasPermission)
                {
                    _logger.LogInformation(" Admin {UserId} có permission {Permission} - Cho phép truy cập", userId, permissionName);
                    context.Succeed(requirement);
                    return;
                }
            }

            _logger.LogWarning("Admin {UserId} KHÔNG CÓ permission: {Permissions} - Từ chối truy cập", 
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

