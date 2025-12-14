using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsPoliciesForLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$4ALdkilcg4FlDaqHDp/AOul0eyGkZFgFjM2d4OsYchU5XCn3JDd1C");

            // ============================================================
            // RLS POLICIES CHO LESSONS TABLE
            // 
            // Lessons thuộc về Courses, nên authorization dựa trên:
            // - Admin: Full access tất cả lessons
            // - Teacher: Chỉ access lessons trong own courses
            // - Student: Chỉ SELECT lessons trong enrolled courses
            // ============================================================

            // Bật Row Level Security cho bảng Lessons
            migrationBuilder.Sql(@"
                ALTER TABLE ""Lessons"" ENABLE ROW LEVEL SECURITY;
            ");

            // ============================================================
            // ADMIN POLICIES - Full CRUD
            // ============================================================

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_admin_select
                ON ""Lessons"" FOR SELECT
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_admin_insert
                ON ""Lessons"" FOR INSERT
                WITH CHECK (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_admin_update
                ON ""Lessons"" FOR UPDATE
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_admin_delete
                ON ""Lessons"" FOR DELETE
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            // ============================================================
            // TEACHER POLICIES - Own courses only
            // Lessons thuộc course mà teacher sở hữu (JOIN qua Courses)
            // ============================================================

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_teacher_select_own
                ON ""Lessons"" FOR SELECT
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                          AND ""Courses"".""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_teacher_insert_own
                ON ""Lessons"" FOR INSERT
                WITH CHECK (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                          AND ""Courses"".""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_teacher_update_own
                ON ""Lessons"" FOR UPDATE
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                          AND ""Courses"".""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                )
                WITH CHECK (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                          AND ""Courses"".""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_teacher_delete_own
                ON ""Lessons"" FOR DELETE
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                          AND ""Courses"".""TeacherId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            // ============================================================
            // STUDENT POLICIES - Enrolled courses only (SELECT only)
            // Student chỉ xem lessons trong courses đã enroll
            // ============================================================

            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_student_select_enrolled
                ON ""Lessons"" FOR SELECT
                USING (
                    current_setting('app.current_user_role', true) = 'Student'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        INNER JOIN ""UserCourses"" ON ""UserCourses"".""CourseId"" = ""Courses"".""CourseId""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                          AND ""UserCourses"".""UserId"" = current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            // ============================================================
            // PERFORMANCE INDEXES
            // Tối ưu cho các JOIN với Courses table
            // ============================================================

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_lessons_course_id
                ON ""Lessons"" (""CourseId"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_lessons_course_order
                ON ""Lessons"" (""CourseId"", ""OrderIndex"");
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
                value: "$2a$11$OWMalfDvscfiLsidJd/nGeNFRYReyw75.DIvYWmgiv4EMYd6UPEPS");

            // Rollback: Xóa indexes và policies theo thứ tự ngược lại

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_lessons_course_order;
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_lessons_course_id;
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_student_select_enrolled ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_teacher_delete_own ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_teacher_update_own ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_teacher_insert_own ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_teacher_select_own ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_admin_delete ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_admin_update ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_admin_insert ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS lessons_policy_admin_select ON ""Lessons"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Lessons"" DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
