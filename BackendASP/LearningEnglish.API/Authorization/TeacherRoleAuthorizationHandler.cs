using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.API.Authorization
{
    // Authorization handler để kiểm tra role Teacher trong database
    // Logic:
    // - Extract userId từ JWT claims
    // - Query database để check role Teacher (realtime, không tin JWT)
    // - Tương tự cách RLS hoạt động - verify từ DB
    
    public class TeacherRoleAuthorizationHandler : AuthorizationHandler<TeacherRoleRequirement>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TeacherRoleAuthorizationHandler> _logger;

        public TeacherRoleAuthorizationHandler(
            IUserRepository userRepository,
            ILogger<TeacherRoleAuthorizationHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TeacherRoleRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("User chưa authenticated");
                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 1: EXTRACT USERID TỪ JWT CLAIMS
            // ═══════════════════════════════════════════════════════════════
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("❌ Không tìm thấy userId trong claims. Claims: {Claims}", 
                    string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}")));
                return;
            }

            _logger.LogInformation("🔍 Checking Teacher role for UserId: {UserId} (from database)", userId);

            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 2: KIỂM TRA ROLE TEACHER TRONG DATABASE
            // ═══════════════════════════════════════════════════════════════
            // Query database để check role (realtime, không tin JWT token)
            var hasTeacherRole = await _userRepository.HasTeacherRoleAsync(userId);
            
            if (hasTeacherRole)
            {
                _logger.LogInformation("✅ User {UserId} có role Teacher trong database - Cho phép truy cập", userId);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("❌ User {UserId} KHÔNG CÓ role Teacher trong database - Từ chối truy cập", userId);
            }
        }
    }

    // Requirement cho Teacher role authorization
    public class TeacherRoleRequirement : IAuthorizationRequirement
    {
        public TeacherRoleRequirement()
        {
            // Không cần tham số, chỉ cần check role Teacher
        }
    }
}

