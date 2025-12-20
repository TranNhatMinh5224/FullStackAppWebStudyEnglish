using System.Security.Claims;

namespace LearningEnglish.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the highest priority role for the user.
        /// Priority: Admin > Teacher > Student
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal</param>
        /// <returns>The highest priority role name, or empty string if no role found</returns>
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

        /// <summary>
        /// Gets all roles for the user
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal</param>
        /// <returns>List of role names</returns>
        public static List<string> GetAllRoles(this ClaimsPrincipal principal)
        {
            return principal.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }

        /// <summary>
        /// Checks if user has a specific role
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal</param>
        /// <param name="roleName">Role name to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        public static bool HasRole(this ClaimsPrincipal principal, string roleName)
        {
            return principal.FindAll(ClaimTypes.Role)
                .Any(c => c.Value.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if user is an Admin
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.HasRole("Admin");
        }

        /// <summary>
        /// Checks if user is a Teacher (may also be Student)
        /// </summary>
        public static bool IsTeacher(this ClaimsPrincipal principal)
        {
            return principal.HasRole("Teacher");
        }

        /// <summary>
        /// Checks if user is a Student
        /// </summary>
        public static bool IsStudent(this ClaimsPrincipal principal)
        {
            return principal.HasRole("Student");
        }
    }
}
