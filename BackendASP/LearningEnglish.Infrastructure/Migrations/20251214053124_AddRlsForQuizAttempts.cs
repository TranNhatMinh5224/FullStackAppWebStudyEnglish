using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsForQuizAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Ge2Lbbxru0NdYHBUZM06Fem1ZOfMbhqdayD1DVJVppHbRUll.C0E2");

            // ========================================
            // RLS for QuizAttempts
            // ========================================
            
            // Enable RLS on QuizAttempts table
            migrationBuilder.Sql(@"
                ALTER TABLE ""QuizAttempts"" ENABLE ROW LEVEL SECURITY;
            ");

            // Drop existing policies if any (for idempotency)
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS quiz_attempts_admin_all ON ""QuizAttempts"";
                DROP POLICY IF EXISTS quiz_attempts_teacher_view ON ""QuizAttempts"";
                DROP POLICY IF EXISTS quiz_attempts_student_own ON ""QuizAttempts"";
            ");

            // Policy 1: Admin - Full access to all quiz attempts
            migrationBuilder.Sql(@"
                CREATE POLICY quiz_attempts_admin_all ON ""QuizAttempts""
                FOR ALL
                TO PUBLIC
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            // Policy 2: Teacher - Full access to attempts for quizzes in their courses
            // 4-level JOIN: QuizAttempts → Quizzes → Assessments → Modules → Lessons → Courses
            migrationBuilder.Sql(@"
                CREATE POLICY quiz_attempts_teacher_view ON ""QuizAttempts""
                FOR ALL
                TO PUBLIC
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1 
                        FROM ""Quizzes"" q
                        INNER JOIN ""Assessments"" a ON q.""AssessmentId"" = a.""AssessmentId""
                        INNER JOIN ""Modules"" m ON a.""ModuleId"" = m.""ModuleId""
                        INNER JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        INNER JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE ""QuizAttempts"".""QuizId"" = q.""QuizId""
                        AND c.""TeacherId"" = current_setting('app.current_user_id')::INTEGER
                    )
                );
            ");

            // Policy 3: Student - Full access to their own quiz attempts
            migrationBuilder.Sql(@"
                CREATE POLICY quiz_attempts_student_own ON ""QuizAttempts""
                FOR ALL
                TO PUBLIC
                USING (
                    current_setting('app.current_user_role', true) = 'Student'
                    AND ""UserId"" = current_setting('app.current_user_id')::INTEGER
                );
            ");

            // Create indexes for performance optimization
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_quiz_attempts_quiz_id 
                ON ""QuizAttempts""(""QuizId"");
                
                CREATE INDEX IF NOT EXISTS idx_quiz_attempts_user_id 
                ON ""QuizAttempts""(""UserId"");
                
                CREATE INDEX IF NOT EXISTS idx_quiz_attempts_status 
                ON ""QuizAttempts""(""Status"");
                
                CREATE INDEX IF NOT EXISTS idx_quiz_attempts_submitted_at 
                ON ""QuizAttempts""(""SubmittedAt"");
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
                value: "$2a$11$g0X7xl.1a5UymJE6LrEE6O4WT8AfHFj3MvJJsIduHovY1cGCSC08G");

            // Drop indexes
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_quiz_attempts_submitted_at;
                DROP INDEX IF EXISTS idx_quiz_attempts_status;
                DROP INDEX IF EXISTS idx_quiz_attempts_user_id;
                DROP INDEX IF EXISTS idx_quiz_attempts_quiz_id;
            ");

            // Drop RLS policies
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS quiz_attempts_student_own ON ""QuizAttempts"";
                DROP POLICY IF EXISTS quiz_attempts_teacher_view ON ""QuizAttempts"";
                DROP POLICY IF EXISTS quiz_attempts_admin_all ON ""QuizAttempts"";
            ");

            // Disable RLS
            migrationBuilder.Sql(@"
                ALTER TABLE ""QuizAttempts"" DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
