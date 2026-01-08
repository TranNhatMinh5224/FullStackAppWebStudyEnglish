using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Constants;

namespace LearningEnglish.API.Authorization
{
    // ==================================================================================
    // PERMISSION AUTHORIZATION HANDLER
    // ==================================================================================
    // Đây là CORE LOGIC của hệ thống Permission-Based Authorization
    // 
    // VAI TRÒ:
    // - Kiểm tra xem User có Permission cần thiết để truy cập endpoint hay không
    // - Được gọi MỖI KHI có request đến endpoint có [RequirePermission] attribute
    //
    // CÁCH HOẠT ĐỘNG:
    // 1. Nhận PermissionRequirement từ PermissionPolicyProvider
    // 2. Lấy thông tin User từ JWT token (đã được parse bởi Authentication Middleware)
    // 3. Query Database để check User có Permission không (qua Roles)
    // 4. Quyết định Cho phép (Succeed) hoặc Từ chối (Fail)
    //
    // FLOW:
    // Request → JWT Auth → Routing → Authorization → PermissionPolicyProvider 
    //   → PermissionAuthorizationHandler (FILE NÀY) → Query DB → Allow/Deny
    // ==================================================================================
    
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        // DEPENDENCIES (Injected qua Constructor)
        private readonly IRolePermissionRepository _rolePermissionRepository; // Query DB để check permissions
        private readonly IHttpContextAccessor _httpContextAccessor;          // Access HttpContext (ít dùng)
        private readonly ILogger<PermissionAuthorizationHandler> _logger;    // Logging để debug

        // CONSTRUCTOR - ASP.NET Core tự động inject dependencies
        public PermissionAuthorizationHandler(
            IRolePermissionRepository rolePermissionRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PermissionAuthorizationHandler> logger)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // ==================================================================================
        // METHOD CHÍNH - XỬ LÝ AUTHORIZATION
        // ==================================================================================
        // Method này được ASP.NET Core gọi khi cần check permission
        //
        // INPUT:
        // - context: Chứa thông tin về User (từ JWT), HttpContext, v.v.
        // - requirement: Chứa danh sách Permissions cần có (từ [RequirePermission])
        //
        // OUTPUT:
        // - Không return gì (void/Task)
        // - Gọi context.Succeed(requirement) nếu CHO PHÉP
        // - Không gọi gì nếu TỪ CHỐI (mặc định là fail)
        // ==================================================================================
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // ============================================================
            // KIỂM TRA 1: User đã đăng nhập chưa?
            // ============================================================
            // context.User: ClaimsPrincipal được tạo từ JWT token
            // context.User.Identity.IsAuthenticated: true nếu có valid token
            //
            // Nếu không authenticated → FAIL ngay (không cho phép)
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("User chưa authenticated");
                return; // DỪNG LẠI - Mặc định là FAIL (403 Forbidden)
            }

            // ============================================================
            // LẤY DANH SÁCH ROLES CỦA USER (từ JWT Claims)
            // ============================================================
            // ClaimTypes.Role: Tên chuẩn của claim chứa role
            // User có thể có NHIỀU roles: ["Teacher", "ContentAdmin"]
            //
            // FindAll(): Tìm TẤT CẢ claims có type = ClaimTypes.Role
            // Select(c => c.Value): Lấy giá trị của claim (tên role)
            var roles = context.User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Log để debug - biết user có roles gì và cần permissions gì
            _logger.LogInformation(" Checking permissions. User roles: {Roles}, Required permissions: {Permissions}", 
                string.Join(", ", roles), string.Join(", ", requirement.Permissions));

            // ============================================================
            // KIỂM TRA 2: User có phải SuperAdmin không?
            // ============================================================
            // SuperAdmin: Vai trò đặc biệt có TOÀN QUYỀN
            // Nếu là SuperAdmin → CHO PHÉP NGAY, không cần check permissions
            //
            // RoleConstants.IsSuperAdmin(): Kiểm tra role name == "SuperAdmin"
            // Any(): Trả về true nếu có ít nhất 1 role là SuperAdmin
            if (roles.Any(r => RoleConstants.IsSuperAdmin(r)))
            {
                _logger.LogInformation(" SuperAdmin tự động có quyền truy cập (toàn quyền)");
                context.Succeed(requirement); // ✅ CHO PHÉP
                return; // DỪNG LẠI - Không cần check tiếp
            }

            // ============================================================
            // LẤY USER ID TỪ JWT CLAIMS
            // ============================================================
            // ClaimTypes.NameIdentifier: Claim chuẩn chứa UserId (thường là "sub")
            // ?? "sub": Fallback nếu không tìm thấy NameIdentifier
            //
            // FindFirst(): Tìm claim ĐẦU TIÊN có type khớp
            // ?.Value: Lấy giá trị của claim (null-safe)
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst("sub")?.Value;

            // Parse userId từ string → int
            // Nếu không parse được hoặc null → FAIL
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                // Log TẤT CẢ claims để debug (trường hợp JWT config sai)
                _logger.LogWarning(" Không tìm thấy userId trong claims. Claims: {Claims}", 
                    string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}")));
                return; // DỪNG LẠI - FAIL
            }

            // Log để debug - biết đang check cho user nào
            _logger.LogInformation("Checking permissions for UserId: {UserId} (Roles: {Roles})", 
                userId, string.Join(", ", roles));

            // ============================================================
            // KIỂM TRA 3: LOOP QUA TỪNG PERMISSION (LOGIC OR)
            // ============================================================
            // requirement.Permissions: Danh sách permissions từ [RequirePermission]
            // Ví dụ: [RequirePermission("Course.Create", "Lesson.Edit")]
            //      → requirement.Permissions = ["Course.Create", "Lesson.Edit"]
            //
            // LOGIC OR: Chỉ cần CÓ 1 permission trong list là CHO PHÉP
            // (Khác với AND logic - cần có TẤT CẢ permissions)
            foreach (var permissionName in requirement.Permissions)
            {
                // ========================================================
                // QUERY DATABASE - KIỂM TRA USER CÓ PERMISSION NÀY KHÔNG
                // ========================================================
                // UserHasPermissionAsync(userId, permissionName):
                // 
                // SQL TƯƠNG ĐƯƠNG:
                // SELECT COUNT(*) > 0
                // FROM RolePermissions rp
                // JOIN Permissions p ON rp.PermissionId = p.PermissionId
                // JOIN Roles r ON rp.RoleId = r.RoleId
                // JOIN UserRoles ur ON r.RoleId = ur.RoleId
                // WHERE p.Name = 'Course.Create'
                //   AND ur.UserId = 123
                //
                // Trả về: true nếu User (qua Roles) có Permission này
                //         false nếu không có
                var hasPermission = await _rolePermissionRepository.UserHasPermissionAsync(userId, permissionName);
                
                // Log kết quả check từng permission
                _logger.LogInformation(" User {UserId} - Permission '{Permission}': {HasPermission}", 
                    userId, permissionName, hasPermission ? " CÓ" : "KHÔNG CÓ");
                
                // ====================================================
                // NẾU CÓ PERMISSION → CHO PHÉP NGAY
                // ====================================================
                if (hasPermission)
                {
                    _logger.LogInformation(" User {UserId} có permission {Permission} - Cho phép truy cập", userId, permissionName);
                    context.Succeed(requirement); // ✅ CHO PHÉP
                    return; // DỪNG LẠI - Không check permissions còn lại (vì đã đủ điều kiện)
                }
            }

            // ============================================================
            // KIỂM TRA 4: NẾU HẾT LOOP MÀ KHÔNG CÓ PERMISSION NÀO
            // ============================================================
            // Không gọi context.Succeed() → Mặc định là FAIL
            // ASP.NET Core sẽ trả về 403 Forbidden cho client
            _logger.LogWarning("User {UserId} KHÔNG CÓ permission: {Permissions} - Từ chối truy cập", 
                userId, string.Join(", ", requirement.Permissions));
            
            // Không cần gọi context.Fail() - mặc định đã là fail
            // return; (implicit)
        }
    }

    // ==================================================================================
    // PERMISSION REQUIREMENT - Class đại diện cho yêu cầu authorization
    // ==================================================================================
    // Đây là "requirement" trong ASP.NET Core Authorization framework
    // 
    // VAI TRÒ:
    // - Chứa danh sách Permissions cần có để truy cập endpoint
    // - Được tạo bởi PermissionPolicyProvider
    // - Được truyền vào PermissionAuthorizationHandler để xử lý
    //
    // FLOW:
    // PermissionPolicyProvider tạo requirement → Handler nhận requirement → Check permissions
    // ==================================================================================
    public class PermissionRequirement : IAuthorizationRequirement
    {
        // Danh sách permissions cần có
        // Ví dụ: ["Course.Create", "Lesson.Edit"]
        public List<string> Permissions { get; }

        // CONSTRUCTOR - Nhận danh sách permission names
        // params: Cho phép gọi với nhiều tham số
        // Ví dụ: new PermissionRequirement("Course.Create", "Lesson.Edit")
        public PermissionRequirement(params string[] permissions)
        {
            // Chuyển array → List để dễ xử lý
            Permissions = permissions.ToList();
        }
    }
}

