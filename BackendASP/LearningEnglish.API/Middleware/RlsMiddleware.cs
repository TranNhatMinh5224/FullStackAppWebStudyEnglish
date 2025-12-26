using System.Security.Claims;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.API.Middleware
{
    // ============================================================================
    // RLS MIDDLEWARE - TỰ ĐỘNG THIẾT LẬP CONTEXT CHO ROW-LEVEL SECURITY
    // ============================================================================
    // 
    // NGUYÊN TẮC RLS:
    // - RLS sinh ra để DB tự quyết định quyền
    // - Middleware KHÔNG cần biết role là gì
    // - Middleware CHỈ làm 1 việc: JWT → userId → set app.current_user_id
    // - DB policies tự check role từ bảng Users và Roles (realtime)
    // 
    // CHỨC NĂNG:
    // - Middleware này chạy ở đầu mỗi HTTP request (sau Authentication, TRƯỚC Authorization)
    // - Tự động extract userId từ JWT token
    // - Gọi DbContext.SetUserContextAsync() để set PostgreSQL session variable
    // - Các RLS policies sử dụng variable này để filter data tự động
    // 
    // EXECUTION ORDER:
    // 1. Request → Authentication Middleware (validate JWT)
    // 2. Request → RLS Middleware (set context) ⬅️ ĐÂY (TRƯỚC Authorization!)
    // 3. Request → Authorization Middleware (check [Authorize])
    // 4. Request → Controller → Service → Repository → Database
    // 
    // VÍ DỤ:
    // HTTP Request với header:
    //   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    // 
    // JWT Token chứa:
    //   {
    //     "sub": "123",              // User ID
    //     "email": "teacher@edu.vn"
    //   }
    // 
    // RLS Middleware sẽ:
    //   → Extract: userId = 123 từ JWT
    //   → Call: dbContext.SetUserContextAsync(123)
    //   → PostgreSQL: SET LOCAL app.current_user_id = '123'
    //
    // Khi query database:
    //   SELECT * FROM "Courses"
    //   → RLS policy tự động:
    //      - Lấy userId từ current_setting('app.current_user_id', true)
    //      - Check role từ bảng Users và Roles (realtime, DB tự quyết định)
    //      - Filter: WHERE "TeacherId" = 123 AND EXISTS (SELECT 1 FROM Users...)
    //   → Chỉ trả về courses của teacher này!
    //
    public class RlsMiddleware
    {
        // RequestDelegate: Reference đến middleware tiếp theo trong pipeline
        private readonly RequestDelegate _next;
        
        // Logger: Để log thông tin debug và error
        private readonly ILogger<RlsMiddleware> _logger;

        // Constructor: ASP.NET Core tự động inject dependencies
        public RlsMiddleware(
            RequestDelegate next, 
            ILogger<RlsMiddleware> logger)
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
            // NGUYÊN TẮC: Middleware CHỈ làm 1 việc duy nhất
            // JWT → lấy userId → set app.current_user_id
            // KHÔNG query DB, KHÔNG load user, KHÔNG load role, KHÔNG business logic
            // DB policies tự check role từ bảng Users và Roles (realtime)
            // ═══════════════════════════════════════════════════════════════
            
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    // Extract userId từ JWT token
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? context.User.FindFirst("sub")?.Value;

                    if (!string.IsNullOrEmpty(userIdClaim) && 
                        int.TryParse(userIdClaim, out int userId))
                    {
                        // CHỈ set userId, KHÔNG làm gì khác
                        // DB policies sẽ tự check role từ bảng Users và Roles
                        await dbContext.SetUserContextAsync(userId);

                        _logger.LogDebug(
                            "RLS Context set: UserId={UserId}, Path={Path}",
                            userId,
                            context.Request.Path
                        );
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Could not extract userId from JWT token. UserId={UserId}",
                            userIdClaim ?? "NULL"
                        );
                    }
                }
                catch (Exception ex)
                {
                    // KHÔNG throw exception (không block request)
                    // RLS policies sẽ xử lý unauthorized access ở database level
                    _logger.LogError(ex, "Error setting RLS context for user");
                }
            }
            // else: User chưa authenticated (anonymous request)
            //       → Không set RLS context
            //       → RLS policies sẽ filter hết data (trả về empty)

            // Tiếp tục đến middleware tiếp theo
            await _next(context);
        }
    }

    // Extension method để đăng ký RLS middleware vào application pipeline
    // Sử dụng trong Program.cs: app.UseRlsMiddleware();
    public static class RlsMiddlewareExtensions
    {
        // Đăng ký RLS Middleware vào pipeline
        // ⚠️ QUAN TRỌNG: Phải gọi AFTER UseAuthentication() và BEFORE UseAuthorization()
        //
        // LÝ DO: Authorization có thể query DB, cần RLS context đã được set trước
        //
        // VÍ DỤ trong Program.cs:
        // app.UseAuthentication();  // 1. Validate JWT
        // app.UseRlsMiddleware();   // 2. Set RLS context ← ĐÂY (TRƯỚC Authorization!)
        // app.UseAuthorization();   // 3. Check [Authorize] (có thể query DB)
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
