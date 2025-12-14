using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsForEssaySubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$g0X7xl.1a5UymJE6LrEE6O4WT8AfHFj3MvJJsIduHovY1cGCSC08G");

            // ========================================
            // RLS for EssaySubmissions
            // ========================================
            
            // Enable RLS on EssaySubmissions table
            migrationBuilder.Sql(@"
                ALTER TABLE ""EssaySubmissions"" ENABLE ROW LEVEL SECURITY;
            ");

            // Drop existing policies if any (for idempotency)
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS essay_submissions_admin_all ON ""EssaySubmissions"";
                DROP POLICY IF EXISTS essay_submissions_teacher_view ON ""EssaySubmissions"";
                DROP POLICY IF EXISTS essay_submissions_student_own ON ""EssaySubmissions"";
            ");

            // Policy 1: Admin - Full access to all essay submissions
            migrationBuilder.Sql(@"
                CREATE POLICY essay_submissions_admin_all ON ""EssaySubmissions""
                FOR ALL
                TO PUBLIC
                USING (
                    EXISTS (
                        SELECT 1 FROM ""Users"" 
                        WHERE ""UserId"" = current_setting('app.current_user_id')::INTEGER
                        AND ""Role"" = 'Admin'
                    )
                );
            ");

            // Policy 2: Teacher - Full access to submissions for essays in their courses
            // 4-level JOIN: EssaySubmissions → Essays → Assessments → Modules → Lessons → Courses
            migrationBuilder.Sql(@"
                CREATE POLICY essay_submissions_teacher_view ON ""EssaySubmissions""
                FOR ALL
                TO PUBLIC
                USING (
                    EXISTS (
                        SELECT 1 
                        FROM ""Essays"" e
                        INNER JOIN ""Assessments"" a ON e.""AssessmentId"" = a.""AssessmentId""
                        INNER JOIN ""Modules"" m ON a.""ModuleId"" = m.""ModuleId""
                        INNER JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        INNER JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE ""EssaySubmissions"".""EssayId"" = e.""EssayId""
                        AND c.""TeacherId"" = current_setting('app.current_user_id')::INTEGER
                    )
                );
            ");

            // Policy 3: Student - Full access to their own submissions
            migrationBuilder.Sql(@"
                CREATE POLICY essay_submissions_student_own ON ""EssaySubmissions""
                FOR ALL
                TO PUBLIC
                USING (
                    ""UserId"" = current_setting('app.current_user_id')::INTEGER
                );
            ");

            // Create indexes for performance optimization
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_essay_submissions_essay_id 
                ON ""EssaySubmissions""(""EssayId"");
                
                CREATE INDEX IF NOT EXISTS idx_essay_submissions_user_id 
                ON ""EssaySubmissions""(""UserId"");
                
                CREATE INDEX IF NOT EXISTS idx_essay_submissions_status 
                ON ""EssaySubmissions""(""Status"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Drg6cR3uqT.I.spDhniucuJ8loazt18.CEYdlZDL1J3gpurhvOdnu");

            // Drop indexes
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_essay_submissions_status;
                DROP INDEX IF EXISTS idx_essay_submissions_user_id;
                DROP INDEX IF EXISTS idx_essay_submissions_essay_id;
            ");

            // Drop RLS policies
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS essay_submissions_student_own ON ""EssaySubmissions"";
                DROP POLICY IF EXISTS essay_submissions_teacher_view ON ""EssaySubmissions"";
                DROP POLICY IF EXISTS essay_submissions_admin_all ON ""EssaySubmissions"";
            ");

            // Disable RLS
            migrationBuilder.Sql(@"
                ALTER TABLE ""EssaySubmissions"" DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
