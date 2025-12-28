namespace LearningEnglish.Application.Common.Constants
{
    /// <summary>
    /// Constants cho các Role names trong hệ thống
    /// Sử dụng để tránh hardcoded strings và dễ bảo trì
    /// </summary>
    public static class RoleConstants
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string ContentAdmin = "ContentAdmin";
        public const string FinanceAdmin = "FinanceAdmin";
        public const string Teacher = "Teacher";
        public const string Student = "Student";

        /// <summary>
        /// Danh sách tất cả Admin roles (không bao gồm SuperAdmin)
        /// </summary>
        public static readonly string[] AdminRoles = new[]
        {
            ContentAdmin,
            FinanceAdmin
        };

        /// <summary>
        /// Danh sách tất cả Admin roles (bao gồm SuperAdmin)
        /// </summary>
        public static readonly string[] AllAdminRoles = new[]
        {
            SuperAdmin,
            ContentAdmin,
            FinanceAdmin
        };

        /// <summary>
        /// Kiểm tra xem role name có phải là Admin role không (bao gồm SuperAdmin)
        /// </summary>
        public static bool IsAdminRole(string roleName)
        {
            return AllAdminRoles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Kiểm tra xem role name có phải là SuperAdmin không
        /// </summary>
        public static bool IsSuperAdmin(string roleName)
        {
            return SuperAdmin.Equals(roleName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// String để dùng trong [Authorize(Roles = "...")] attribute
        /// Cho phép tất cả Admin roles truy cập
        /// </summary>
        public const string AllAdminRolesString = "SuperAdmin, ContentAdmin, FinanceAdmin";
    }
}

