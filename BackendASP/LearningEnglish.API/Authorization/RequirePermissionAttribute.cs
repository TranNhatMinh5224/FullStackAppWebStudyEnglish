using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Authorization
{
    // ATTRIBUTE NÀY DÙNG ĐỂ BẢO VỆ CONTROLLER/ACTION
    // Khi gắn [RequirePermission("Course.Create")] lên một endpoint
    // → User PHẢI có permission "Course.Create" mới được truy cập
    //
    // VÍ DỤ SỬ DỤNG:
    // [RequirePermission("Course.Create")]
    // public async Task<IActionResult> CreateCourse() { }
    //
    // KẾ THỪA TỪ AuthorizeAttribute:
    // - Attribute chuẩn của ASP.NET Core cho authorization
    // - Có property "Policy" để chỉ định policy name cần check
    
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        // TIỀN TỐ CỐ ĐỊNH CHO TẤT CẢ PERMISSION POLICIES
        // Mục đích: Để PermissionPolicyProvider nhận biết đây là permission-based policy
        // VD: Policy name sẽ là "PERMISSION_Course.Create" thay vì chỉ "Course.Create"
        private const string POLICY_PREFIX = "PERMISSION_";

        // CONSTRUCTOR - Được gọi khi bạn viết [RequirePermission(...)]
        // 
        // THAM SỐ:
        // - params string[] permissions: Danh sách permissions cần có (có thể 1 hoặc nhiều)
        //   + Ví dụ 1: [RequirePermission("Course.Create")] 
        //     → permissions = ["Course.Create"]
        //   + Ví dụ 2: [RequirePermission("Course.Create", "Lesson.Edit")]
        //     → permissions = ["Course.Create", "Lesson.Edit"]
        //
        // CÁCH HOẠT ĐỘNG:
        // 1. Nối các permission names bằng dấu "_"
        // 2. Thêm PREFIX "PERMISSION_" vào đầu
        // 3. Set vào property Policy (của AuthorizeAttribute cha)
        // 4. Lưu lại permissions vào property để có thể truy cập sau
        public RequirePermissionAttribute(params string[] permissions)
        {
            // BƯỚC 1: TẠO TÊN POLICY
            // string.Join("_", permissions): Nối các permission bằng "_"
            // Ví dụ: ["Course.Create", "Lesson.Edit"] → "Course.Create_Lesson.Edit"
            // 
            // Thêm PREFIX: "PERMISSION_" + "Course.Create_Lesson.Edit"
            //            = "PERMISSION_Course.Create_Lesson.Edit"
            //
            // TẠI SAO CẦN TÊN POLICY NÀY?
            // - ASP.NET sẽ gửi policy name này cho IAuthorizationPolicyProvider
            // - PermissionPolicyProvider sẽ parse chuỗi này để:
            //   + Nhận biết đây là permission policy (nhờ PREFIX)
            //   + Tách ra được danh sách permissions cần check
            Policy = $"{POLICY_PREFIX}{string.Join("_", permissions)}";
            
            // BƯỚC 2: LƯU PERMISSIONS VÀO PROPERTY
            // Mục đích: Để code khác có thể đọc được permissions nếu cần
            // (Tuy nhiên trong flow authorization, permissions được parse từ Policy name)
            Permissions = permissions;
        }

        // PROPERTY PUBLIC ĐỂ TRUY CẬP DANH SÁCH PERMISSIONS
        // Có thể dùng để:
        // - Logging: Biết endpoint này cần permissions gì
        // - Debug: Kiểm tra attribute đã được config đúng chưa
        //
        public string[] Permissions { get; }
    }
}


