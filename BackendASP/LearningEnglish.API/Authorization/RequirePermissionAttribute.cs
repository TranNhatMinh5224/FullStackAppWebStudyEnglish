using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Authorization
{

    // Attribute để yêu cầu permission cụ thể cho Admin
    // SuperAdmin tự động pass, Admin cần có permission
    // 
    // Sử dụng: [RequirePermission("Admin.Course.Manage")]
    
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        private const string POLICY_PREFIX = "PERMISSION_";

        public RequirePermissionAttribute(params string[] permissions)
        {
            Policy = $"{POLICY_PREFIX}{string.Join("_", permissions)}";
            Permissions = permissions;
        }

        public string[] Permissions { get; }
    }
}

