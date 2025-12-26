using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Authorization
{
    // Attribute để yêu cầu role Teacher (check từ database, không tin JWT token)
    // Sử dụng: [RequireTeacherRole]
    // 
    // Logic:
    // - Kiểm tra role Teacher trong database (realtime)
    // - Không dựa vào JWT token (an toàn hơn khi role thay đổi)
    // - Tương tự cách RLS hoạt động - verify từ DB
    
    public class RequireTeacherRoleAttribute : AuthorizeAttribute
    {
        private const string POLICY_NAME = "REQUIRE_TEACHER_ROLE";

        public RequireTeacherRoleAttribute()
        {
            Policy = POLICY_NAME;
        }
    }
}

