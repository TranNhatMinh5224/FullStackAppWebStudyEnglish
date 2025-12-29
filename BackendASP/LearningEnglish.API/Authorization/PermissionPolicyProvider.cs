using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LearningEnglish.API.Authorization
{
  
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private const string POLICY_PREFIX = "PERMISSION_";
        private const string TEACHER_ROLE_POLICY = "REQUIRE_TEACHER_ROLE";
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX))
            {
                var permissionNames = policyName
                    .Substring(POLICY_PREFIX.Length)
                    .Split('_', StringSplitOptions.RemoveEmptyEntries);

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(permissionNames))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            // ═══════════════════════════════════════════════════════════════
            // HANDLE TEACHER ROLE POLICY
            // ═══════════════════════════════════════════════════════════════
            if (policyName == TEACHER_ROLE_POLICY)
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new TeacherRoleRequirement())
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}

