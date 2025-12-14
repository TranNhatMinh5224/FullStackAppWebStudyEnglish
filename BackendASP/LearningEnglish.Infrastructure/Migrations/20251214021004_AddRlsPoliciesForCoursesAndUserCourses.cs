using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsPoliciesForCoursesAndUserCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // RLS POLICIES CHO COURSES + USERCOURSES
            //
            // RLS hoạt động theo từng ROW, không theo API hay service layer.
            // Mỗi policy đại diện cho 1 intent rõ ràng (ai – làm gì – trên row nào).
            //
            // Context được set từ backend (SET LOCAL trong transaction):
            // - app.current_user_role : Admin | Teacher | Student | empty (Guest)
            // - app.current_user_id   : Id của user hiện tại
            // ============================================================

            // ====================================================================================
            // COURSES TABLE
            // ====================================================================================

            // Bật Row Level Security cho bảng Courses
            // Sau khi bật, mọi truy vấn đều phải thỏa ít nhất 1 policy
            migrationBuilder.Sql(@"
                ALTER TABLE ""Courses"" ENABLE ROW LEVEL SECURITY;
            ");

            // ------------------------------------------------------------
            // ADMIN POLICIES
            // Admin có toàn quyền quản lý Courses
            // ------------------------------------------------------------

            // Admin được xem tất cả courses (không giới hạn TeacherId)
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_admin_select
                ON ""Courses"" FOR SELECT
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_admin_insert
                ON ""Courses"" FOR INSERT
                WITH CHECK (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_admin_update
                ON ""Courses"" FOR UPDATE
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_admin_delete
                ON ""Courses"" FOR DELETE
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            // ------------------------------------------------------------
            // TEACHER POLICIES
            // Teacher chỉ được thao tác trên course của chính mình
            // Điều kiện ownership: TeacherId = current_user_id
            // ------------------------------------------------------------

            // Teacher chỉ xem course do mình tạo
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_teacher_select_own
                ON ""Courses"" FOR SELECT
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND ""TeacherId"" = current_setting('app.current_user_id', true)::int
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_teacher_insert_own
                ON ""Courses"" FOR INSERT
                WITH CHECK (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND ""TeacherId"" = current_setting('app.current_user_id', true)::int
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_teacher_update_own
                ON ""Courses"" FOR UPDATE
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND ""TeacherId"" = current_setting('app.current_user_id', true)::int
                )
                WITH CHECK (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND ""TeacherId"" = current_setting('app.current_user_id', true)::int
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_teacher_delete_own
                ON ""Courses"" FOR DELETE
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND ""TeacherId"" = current_setting('app.current_user_id', true)::int
                );
            ");

            // ------------------------------------------------------------
            // STUDENT POLICIES
            // Student không được CRUD course
            // Chỉ được xem các course đã enroll
            // ------------------------------------------------------------

            // Student chỉ xem course mà mình đã đăng ký
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_student_select_enrolled
                ON ""Courses"" FOR SELECT
                USING (
                    current_setting('app.current_user_role', true) = 'Student'
                    AND EXISTS (
                        SELECT 1
                        FROM ""UserCourses""
                        WHERE ""UserCourses"".""CourseId"" = ""Courses"".""CourseId""
                          AND ""UserCourses"".""UserId"" =
                              current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_guest_select_system
                ON ""Courses"" FOR SELECT
                USING (
                    ""Type"" = 1
                    AND (
                        current_setting('app.current_user_role', true) = ''
                        OR current_setting('app.current_user_role', true) IS NULL
                    )
                );
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_courses_teacher_id
                ON ""Courses"" (""TeacherId"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_courses_type
                ON ""Courses"" (""Type"");
            ");

            // ====================================================================================
            // USERCOURSES TABLE
            // ====================================================================================

            // Bật RLS cho bảng UserCourses (enrollment)
            migrationBuilder.Sql(@"
                ALTER TABLE ""UserCourses"" ENABLE ROW LEVEL SECURITY;
            ");

            // ------------------------------------------------------------
            // ADMIN POLICIES
            // Admin quản lý toàn bộ enrollment
            // ------------------------------------------------------------

            // Admin xem tất cả enrollment
            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_admin_select
                ON ""UserCourses"" FOR SELECT
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_admin_insert
                ON ""UserCourses"" FOR INSERT
                WITH CHECK (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_admin_update
                ON ""UserCourses"" FOR UPDATE
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_admin_delete
                ON ""UserCourses"" FOR DELETE
                USING (current_setting('app.current_user_role', true) = 'Admin');
            ");

            // ------------------------------------------------------------
            // TEACHER POLICIES
            // Teacher chỉ được xem học viên của course mình dạy
            // ------------------------------------------------------------

            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_teacher_select_own_courses
                ON ""UserCourses"" FOR SELECT
                USING (
                    current_setting('app.current_user_role', true) = 'Teacher'
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""UserCourses"".""CourseId""
                          AND ""Courses"".""TeacherId"" =
                              current_setting('app.current_user_id', true)::int
                    )
                );
            ");

            // ------------------------------------------------------------
            // STUDENT POLICIES
            // Student chỉ được thao tác enrollment của chính mình
            // ------------------------------------------------------------

            // Student xem enrollment của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_student_select_own
                ON ""UserCourses"" FOR SELECT
                USING (
                    current_setting('app.current_user_role', true) = 'Student'
                    AND ""UserId"" = current_setting('app.current_user_id', true)::int
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_student_insert_own
                ON ""UserCourses"" FOR INSERT
                WITH CHECK (
                    current_setting('app.current_user_role', true) = 'Student'
                    AND ""UserId"" = current_setting('app.current_user_id', true)::int
                );
            ");

            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_student_delete_own
                ON ""UserCourses"" FOR DELETE
                USING (
                    current_setting('app.current_user_role', true) = 'Student'
                    AND ""UserId"" = current_setting('app.current_user_id', true)::int
                );
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_usercourses_user_id
                ON ""UserCourses"" (""UserId"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_usercourses_course_id
                ON ""UserCourses"" (""CourseId"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_usercourses_user_course
                ON ""UserCourses"" (""UserId"", ""CourseId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback toàn bộ index + policy
            // Thứ tự ngược lại để tránh phụ thuộc

            // USERCOURSES
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_usercourses_user_course;
                DROP INDEX IF EXISTS idx_usercourses_course_id;
                DROP INDEX IF EXISTS idx_usercourses_user_id;

                DROP POLICY IF EXISTS usercourses_policy_student_delete_own ON ""UserCourses"";
                DROP POLICY IF EXISTS usercourses_policy_student_insert_own ON ""UserCourses"";
                DROP POLICY IF EXISTS usercourses_policy_student_select_own ON ""UserCourses"";
                DROP POLICY IF EXISTS usercourses_policy_teacher_select_own_courses ON ""UserCourses"";
                DROP POLICY IF EXISTS usercourses_policy_admin_delete ON ""UserCourses"";
                DROP POLICY IF EXISTS usercourses_policy_admin_update ON ""UserCourses"";
                DROP POLICY IF EXISTS usercourses_policy_admin_insert ON ""UserCourses"";
                DROP POLICY IF EXISTS usercourses_policy_admin_select ON ""UserCourses"";

                ALTER TABLE ""UserCourses"" DISABLE ROW LEVEL SECURITY;
            ");

            // COURSES
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_courses_type;
                DROP INDEX IF EXISTS idx_courses_teacher_id;

                DROP POLICY IF EXISTS courses_policy_guest_select_system ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_student_select_enrolled ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_teacher_delete_own ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_teacher_update_own ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_teacher_insert_own ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_teacher_select_own ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_admin_delete ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_admin_update ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_admin_insert ON ""Courses"";
                DROP POLICY IF EXISTS courses_policy_admin_select ON ""Courses"";

                ALTER TABLE ""Courses"" DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
