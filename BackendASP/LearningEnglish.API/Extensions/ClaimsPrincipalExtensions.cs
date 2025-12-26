using System.Security.Claims;

namespace LearningEnglish.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // Lấy role có độ ưu tiên cao nhất của user
        // Độ ưu tiên: Admin > Teacher > Student
        public static string GetPrimaryRole(this ClaimsPrincipal principal)
        {
            var roles = principal.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!roles.Any())
                return string.Empty;

            // Priority order: Admin > Teacher > Student
            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                return "Admin";
            
            if (roles.Contains("Teacher", StringComparer.OrdinalIgnoreCase))
                return "Teacher";
            
            if (roles.Contains("Student", StringComparer.OrdinalIgnoreCase))
                return "Student";

            // Return first role if none of the standard roles found
            return roles.First();
        }

        // Lấy tất cả các role của user
        public static List<string> GetAllRoles(this ClaimsPrincipal principal)
        {
            return principal.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }

        // Kiểm tra xem user có role cụ thể hay không
        public static bool HasRole(this ClaimsPrincipal principal, string roleName)
        {
            return principal.FindAll(ClaimTypes.Role)
                .Any(c => c.Value.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        // Kiểm tra xem user có phải là Admin không
        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.HasRole("Admin");
        }

        // Kiểm tra xem user có phải là Teacher không (có thể đồng thời là Student)
        public static bool IsTeacher(this ClaimsPrincipal principal)
        {
            return principal.HasRole("Teacher");
        }

        // Kiểm tra xem user có phải là Student không
        public static bool IsStudent(this ClaimsPrincipal principal)
        {
            return principal.HasRole("Student");
        }

        // Lấy userId từ claims - Throw exception nếu không có
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? principal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in claims. User is not authenticated.");
            }

            return userId;
        }

        // Lấy userId từ claims - Trả về 0 nếu không có (safe cho AllowAnonymous endpoints)
        public static int GetUserIdSafe(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? principal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return 0;
            }

            return userId;
        }
    }
}

