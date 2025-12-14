using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsPoliciesForModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Drg6cR3uqT.I.spDhniucuJ8loazt18.CEYdlZDL1J3gpurhvOdnu");

            // ============================================================
            // RLS POLICIES CHO MODULES TABLE
            // 
            // Modules thuộc về Lessons → Courses
            // Authorization chain: Module → Lesson → Course → Teacher/Student
            // 
            // Pattern:
            // - Admin: Full access tất cả modules
            // - Teacher: Chỉ access modules trong own courses
            // - Student: Chỉ SELECT modules trong enrolled courses
            // ============================================================

            // Bật Row Level Security cho bảng Modules
            migrationBuilder.Sql(@"
                ALTER TABLE ""Modules"" ENABLE ROW LEVEL SECURITY;
            ");

            // ============================================================
            // ADMIN POLICIES - Full CRUD
            // ============================================================

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_admin_select
                ON ""Modules"" FOR SELECT
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_admin_insert
                ON ""Modules"" FOR INSERT
                WITH CHECK (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_admin_update
                ON ""Modules"" FOR UPDATE
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_admin_delete
                ON ""Modules"" FOR DELETE
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            // ============================================================
            // TEACHER POLICIES - Own courses only
            // Modules → Lessons → Courses (JOIN 2 bước)
            // ============================================================

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_teacher_select_own
                ON ""Modules"" FOR SELECT
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Lessons"" l
                        INNER JOIN ""Courses"" c ON c.""CourseId"" = l.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                          AND c.""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_teacher_insert_own
                ON ""Modules"" FOR INSERT
                WITH CHECK (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Lessons"" l
                        INNER JOIN ""Courses"" c ON c.""CourseId"" = l.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                          AND c.""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_teacher_update_own
                ON ""Modules"" FOR UPDATE
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Lessons"" l
                        INNER JOIN ""Courses"" c ON c.""CourseId"" = l.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                          AND c.""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                )
                WITH CHECK (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Lessons"" l
                        INNER JOIN ""Courses"" c ON c.""CourseId"" = l.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                          AND c.""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_teacher_delete_own
                ON ""Modules"" FOR DELETE
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Lessons"" l
                        INNER JOIN ""Courses"" c ON c.""CourseId"" = l.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                          AND c.""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            // ============================================================
            // STUDENT POLICIES - Enrolled courses only (SELECT only)
            // Modules → Lessons → Courses → UserCourses (JOIN 3 bước)
            // ============================================================

            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_student_select_enrolled
                ON ""Modules"" FOR SELECT
                USING (
                    current_setting('app.current_user_role', true) = 'Student'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Lessons"" l
                        INNER JOIN ""Courses"" c ON c.""CourseId"" = l.""CourseId""
                        INNER JOIN ""UserCourses"" uc ON uc.""CourseId"" = c.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                          AND uc.""UserId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            // ============================================================
            // PERFORMANCE INDEXES
            // Tối ưu cho JOIN với Lessons table
            // ============================================================

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_modules_lesson_id
                ON ""Modules"" (""LessonId"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_modules_lesson_order
                ON ""Modules"" (""LessonId"", ""OrderIndex"");
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
                value: "$2a$11$4ALdkilcg4FlDaqHDp/AOul0eyGkZFgFjM2d4OsYchU5XCn3JDd1C");

            // Rollback: Xóa indexes và policies theo thứ tự ngược lại

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_modules_lesson_order;
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_modules_lesson_id;
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_student_select_enrolled ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_teacher_delete_own ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_teacher_update_own ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_teacher_insert_own ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_teacher_select_own ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_admin_delete ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_admin_update ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_admin_insert ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS modules_policy_admin_select ON ""Modules"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Modules"" DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
