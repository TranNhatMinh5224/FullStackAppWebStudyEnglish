using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller
{
    /// <summary>
    /// Base controller với các helper methods cho user authentication
    /// </summary>
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Lấy UserId từ JWT token
        /// </summary>
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user credentials - UserId not found");
            }

            return userId;
        }

        /// <summary>
        /// Lấy UserId từ JWT token, return 0 nếu không tìm thấy
        /// </summary>
        protected int? GetCurrentUserIdOrNull()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }

            return userId;
        }

        /// <summary>
        /// Lấy primary role của user từ JWT token
        /// </summary>
        protected string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(roleClaim))
            {
                throw new UnauthorizedAccessException("User role not found");
            }

            return roleClaim;
        }

        /// <summary>
        /// Lấy primary role của user từ JWT token, return null nếu không tìm thấy
        /// </summary>
        protected string? GetCurrentUserRoleOrNull()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Lấy email của user từ JWT token
        /// </summary>
        protected string GetCurrentUserEmail()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value
                           ?? User.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(emailClaim))
            {
                throw new UnauthorizedAccessException("User email not found");
            }

            return emailClaim;
        }

        /// <summary>
        /// Kiểm tra user có authenticated không
        /// </summary>
        protected bool IsUserAuthenticated()
        {
            return User.Identity?.IsAuthenticated == true;
        }
    }
}