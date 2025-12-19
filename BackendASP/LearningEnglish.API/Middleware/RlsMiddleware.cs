using System.Security.Claims;
using LearningEnglish.Infrastructure.Data;
using LearningEnglish.API.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.API.Middleware
{
    // ============================================================================
    // RLS MIDDLEWARE - TỰ ĐỘNG THIẾT LẬP CONTEXT CHO ROW-LEVEL SECURITY
    // ============================================================================
    // 
    // CHỨC NĂNG:
    // - Middleware này chạy ở đầu mỗi HTTP request (sau Authentication)
    // - Tự động extract userId và role từ JWT token
    // - Gọi DbContext.SetUserContextAsync() để set PostgreSQL session variables
    // - Các RLS policies sử dụng variables này để filter data tự động
    // 
    // EXECUTION ORDER:
    // 1. Request → Authentication Middleware (validate JWT)
    // 2. Request → Authorization Middleware (check [Authorize])
    // 3. Request → RLS Middleware (set context) ⬅️ ĐÂY
    // 4. Request → Controller → Service → Repository → Database
    // 
    // VÍ DỤ:
    // HTTP Request với header:
    //   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    // 
    // JWT Token chứa:
    //   {
    //     "sub": "123",              // User ID
    //     "role": "Teacher",         // User Role
    //     "email": "teacher@edu.vn"
    //   }
    // 
    // RLS Middleware sẽ:
    //   → Extract: userId = 123, role = "Teacher"
    //   → Call: dbContext.SetUserContextAsync(123, "Teacher")
    //   → PostgreSQL: SET LOCAL app.current_user_id = '123'
    //   → PostgreSQL: SET LOCAL app.current_user_role = 'Teacher'
    // 
    // Khi query database:
    //   SELECT * FROM "Courses"
    //   → RLS policy tự động thêm: WHERE "TeacherId" = 123
    //   → Chỉ trả về courses của teacher này!
    //
    public class RlsMiddleware
    {
        // RequestDelegate: Reference đến middleware tiếp theo trong pipeline
        private readonly RequestDelegate _next;
        
        // Logger: Để log thông tin debug và error
        private readonly ILogger<RlsMiddleware> _logger;

        // Constructor: ASP.NET Core tự động inject dependencies
        public RlsMiddleware(RequestDelegate next, ILogger<RlsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // InvokeAsync: Method chính được gọi khi request đi qua middleware này
        // - context: HttpContext chứa thông tin về request hiện tại
        // - dbContext: AppDbContext được inject tự động (scoped per request)
        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 1: KIỂM TRA XEM USER ĐÃ AUTHENTICATED CHƯA
            // ═══════════════════════════════════════════════════════════════
            
            // context.User: ClaimsPrincipal object chứa thông tin user từ JWT token
            // Identity.IsAuthenticated: true nếu JWT token hợp lệ, false nếu không có token
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    // ═══════════════════════════════════════════════════════════════
                    // BƯỚC 2: EXTRACT USER ID TỪ JWT TOKEN
                    // ═══════════════════════════════════════════════════════════════
                    
                    // JWT token có thể chứa userId ở nhiều claim types khác nhau:
                    // - ClaimTypes.NameIdentifier: Standard claim type của .NET
                    // - "sub": Standard claim type theo JWT specification
                    // Ví dụ JWT payload: { "sub": "123", "role": "Teacher", ... }
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? context.User.FindFirst("sub")?.Value;

                    // ═══════════════════════════════════════════════════════════════
                    // BƯỚC 3: EXTRACT ROLE TỪ JWT TOKEN
                    // ═══════════════════════════════════════════════════════════════
                    
                    // Get primary role (highest priority: Admin > Teacher > Student)
                    // User may have multiple roles, use extension method to get the primary one
                    var roleClaim = context.User.GetPrimaryRole();

                    // ═══════════════════════════════════════════════════════════════
                    // BƯỚC 4: VALIDATE VÀ PARSE DATA
                    // ═══════════════════════════════════════════════════════════════
                    
                    // Kiểm tra:
                    // 1. userIdClaim không null/empty
                    // 2. Parse thành công sang int (userId phải là số)
                    // 3. roleClaim không null/empty
                    if (!string.IsNullOrEmpty(userIdClaim) && 
                        int.TryParse(userIdClaim, out int userId) &&
                        !string.IsNullOrEmpty(roleClaim))
                    {
                        // ═══════════════════════════════════════════════════════════════
                        // BƯỚC 5: SET RLS CONTEXT VARIABLES
                        // ═══════════════════════════════════════════════════════════════
                        
                        // Gọi method SetUserContextAsync trong DbContext
                        // Method này sẽ execute SQL command:
                        //   SELECT set_config('app.current_user_id', '123', true),
                        //          set_config('app.current_user_role', 'Teacher', true)
                        await dbContext.SetUserContextAsync(userId, roleClaim);

                        // ═══════════════════════════════════════════════════════════════
                        // BƯỚC 6: LOG THÔNG TIN DEBUG
                        // ═══════════════════════════════════════════════════════════════
                        
                        // Log để developer có thể verify RLS context đã set đúng chưa
                        // Output ví dụ:
                        // [Debug] RLS Context set: UserId=123, Role=Teacher, Path=/api/courses/my-courses
                        _logger.LogDebug(
                            "RLS Context set: UserId={UserId}, Role={Role}, Path={Path}",
                            userId,           // {UserId}
                            roleClaim,        // {Role}
                            context.Request.Path  // {Path} - Endpoint đang được gọi
                        );
                    }
                    else
                    {
                        // ═══════════════════════════════════════════════════════════════
                        // BƯỚC 7: LOG WARNING NẾU KHÔNG EXTRACT ĐƯỢC
                        // ═══════════════════════════════════════════════════════════════
                        
                        // Trường hợp này xảy ra khi:
                        // - JWT token không có claim "sub" hoặc "NameIdentifier"
                        // - JWT token không có claim "role"
                        // - userId không parse được sang int
                        _logger.LogWarning(
                            "Could not extract user context from JWT token. UserId={UserId}, Role={Role}",
                            userIdClaim ?? "NULL",
                            roleClaim ?? "NULL"
                        );
                    }
                }
                catch (Exception ex)
                {
                    // ═══════════════════════════════════════════════════════════════
                    // BƯỚC 8: XỬ LÝ LỖI (KHÔNG BLOCK REQUEST)
                    // ═══════════════════════════════════════════════════════════════
                    
                    // Nếu có lỗi khi set context (VD: database connection error):
                    // - Log error để admin biết
                    // - KHÔNG throw exception (không block request)
                    // - RLS policies sẽ xử lý unauthorized access ở database level
                    // 
                    // LÝ DO: RLS là defense layer bổ sung, không nên break toàn bộ app
                    _logger.LogError(ex, "Error setting RLS context for user");
                }
            }
            // else: User chưa authenticated (anonymous request)
            //       → Không set RLS context
            //       → RLS policies sẽ filter hết data (trả về empty)

            // ═══════════════════════════════════════════════════════════════
            // BƯỚC 9: TIẾP TỤC ĐẾN MIDDLEWARE TIẾP THEO
            // ═══════════════════════════════════════════════════════════════
            
            // Gọi middleware tiếp theo trong pipeline
            // Thứ tự: RlsMiddleware → Controller → Service → Repository → Database
            await _next(context);
        }
    }

    // Extension method để đăng ký RLS middleware vào application pipeline
    // Sử dụng trong Program.cs: app.UseRlsMiddleware();
    public static class RlsMiddlewareExtensions
    {
        // Đăng ký RLS Middleware vào pipeline
        // ⚠️ QUAN TRỌNG: Phải gọi AFTER UseAuthentication() và BEFORE MapControllers()
        //
        // VÍ DỤ trong Program.cs:
        // app.UseAuthentication();  // 1. Validate JWT
        // app.UseAuthorization();   // 2. Check [Authorize]
        // app.UseRlsMiddleware();   // 3. Set RLS context ← ĐÂY
        // app.MapControllers();     // 4. Execute actions
        public static IApplicationBuilder UseRlsMiddleware(this IApplicationBuilder builder)
        {
            // UseMiddleware<T>: Đăng ký middleware vào pipeline
            // ASP.NET Core sẽ tự động:
            // 1. Tạo instance của RlsMiddleware
            // 2. Inject dependencies (RequestDelegate, ILogger, AppDbContext)
            // 3. Gọi InvokeAsync() khi request đi qua
            return builder.UseMiddleware<RlsMiddleware>();
        }
    }
}
