using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Authorization
{

    
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

