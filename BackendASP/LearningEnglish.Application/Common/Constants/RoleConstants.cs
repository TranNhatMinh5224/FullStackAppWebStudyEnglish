namespace LearningEnglish.Application.Common.Constants
{

  
    public static class RoleConstants
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string ContentAdmin = "ContentAdmin";
        public const string FinanceAdmin = "FinanceAdmin";
        public const string Teacher = "Teacher";
        public const string Student = "Student";

   
        public static readonly string[] AdminRoles = new[]
        {
            ContentAdmin,
            FinanceAdmin
        };

 
        public static readonly string[] AllAdminRoles = new[]
        {
            SuperAdmin,
            ContentAdmin,
            FinanceAdmin
        };

        
        public static bool IsAdminRole(string roleName)
        {
            return AllAdminRoles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

       
        public static bool IsSuperAdmin(string roleName)
        {
            return SuperAdmin.Equals(roleName, StringComparison.OrdinalIgnoreCase);
        }

      
        public const string AllAdminRolesString = "SuperAdmin, ContentAdmin, FinanceAdmin";
    }
}

