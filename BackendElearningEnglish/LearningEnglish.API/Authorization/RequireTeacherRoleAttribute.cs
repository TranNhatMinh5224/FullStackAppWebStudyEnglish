using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Authorization
{

    public class RequireTeacherRoleAttribute : AuthorizeAttribute
    {
        private const string POLICY_NAME = "REQUIRE_TEACHER_ROLE";

        public RequireTeacherRoleAttribute()
        {
            Policy = POLICY_NAME;
        }
    }
}

