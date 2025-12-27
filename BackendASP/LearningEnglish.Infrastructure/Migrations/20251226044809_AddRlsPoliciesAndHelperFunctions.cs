using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsPoliciesAndHelperFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================================
            // TẠO SCHEMA VÀ HELPER FUNCTIONS CHO RLS
            // ============================================================================
            
            migrationBuilder.Sql(@"CREATE SCHEMA IF NOT EXISTS app;");

            // Helper function: Lấy current_user_id từ session variable
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION app.current_user_id() RETURNS INTEGER AS $$
                BEGIN
                    RETURN NULLIF(current_setting('app.current_user_id', true), '')::INTEGER;
                EXCEPTION
                    WHEN OTHERS THEN
                        RETURN NULL;
                END;
                $$ LANGUAGE plpgsql STABLE;
            ");

            // Helper function: Kiểm tra user có role cụ thể không
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION app.user_has_role(role_name TEXT) RETURNS BOOLEAN AS $$
                BEGIN
                    RETURN EXISTS (
                        SELECT 1
                        FROM ""Users"" u
                        JOIN ""UserRoles"" ur ON u.""UserId"" = ur.""UserId""
                        JOIN ""Roles"" r ON ur.""RoleId"" = r.""RoleId""
                        WHERE u.""UserId"" = app.current_user_id()
                        AND r.""Name"" = role_name
                    );
                END;
                $$ LANGUAGE plpgsql STABLE;
            ");

            // Helper function: Kiểm tra user có phải SuperAdmin không
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION app.is_superadmin() RETURNS BOOLEAN AS $$
                BEGIN
                    RETURN app.user_has_role('SuperAdmin');
                END;
                $$ LANGUAGE plpgsql STABLE;
            ");

            // Helper function: Kiểm tra user có permission cụ thể không
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION app.user_has_permission(permission_name TEXT) RETURNS BOOLEAN AS $$
                BEGIN
                    RETURN EXISTS (
                        SELECT 1 
                        FROM ""Users"" u
                        JOIN ""UserRoles"" ur ON u.""UserId"" = ur.""UserId""
                        JOIN ""Roles"" r ON ur.""RoleId"" = r.""RoleId""
                        JOIN ""RolePermissions"" rp ON r.""RoleId"" = rp.""RoleId""
                        JOIN ""Permissions"" p ON rp.""PermissionId"" = p.""PermissionId""
                        WHERE u.""UserId"" = app.current_user_id()
                        AND p.""Name"" = permission_name
                    );
                END;
                $$ LANGUAGE plpgsql STABLE;
            ");

            // ============================================================================
            // ENABLE RLS CHO CÁC TABLES
            // ============================================================================
            
            migrationBuilder.Sql(@"ALTER TABLE ""Courses"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""UserCourses"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""Lessons"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""Modules"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""EssaySubmissions"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""QuizAttempts"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""Payments"" ENABLE ROW LEVEL SECURITY;");

            // ============================================================================
            // RLS POLICIES CHO COURSES
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_superadmin_all
                ON ""Courses"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS (defense in depth)
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_admin_select
                ON ""Courses"" FOR SELECT
                USING (app.user_has_permission('Admin.Course.Manage'));
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_admin_insert
                ON ""Courses"" FOR INSERT
                WITH CHECK (app.user_has_permission('Admin.Course.Manage'));
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_admin_update
                ON ""Courses"" FOR UPDATE
                USING (app.user_has_permission('Admin.Course.Manage'));
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_admin_delete
                ON ""Courses"" FOR DELETE
                USING (app.user_has_permission('Admin.Course.Manage'));
            ");

            // Teacher: Chỉ courses của mình (Type = Teacher = 2)
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_teacher_select
                ON ""Courses"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND ""TeacherId"" = app.current_user_id()
                    AND ""Type"" = 2
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_teacher_insert
                ON ""Courses"" FOR INSERT
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND ""TeacherId"" = app.current_user_id()
                    AND ""Type"" = 2
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_teacher_update
                ON ""Courses"" FOR UPDATE
                USING (
                    app.user_has_role('Teacher')
                    AND ""TeacherId"" = app.current_user_id()
                    AND ""Type"" = 2
                )
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND ""TeacherId"" = app.current_user_id()
                    AND ""Type"" = 2
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_teacher_delete
                ON ""Courses"" FOR DELETE
                USING (
                    app.user_has_role('Teacher')
                    AND ""TeacherId"" = app.current_user_id()
                    AND ""Type"" = 2
                );
            ");

            // Student: Xem system courses (Type = 1) để browse và enroll
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_student_select_system
                ON ""Courses"" FOR SELECT
                USING (
                    app.user_has_role('Student')
                    AND ""Type"" = 1
                );
            ");

            // Student: Xem courses đã đăng ký
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_student_select_enrolled
                ON ""Courses"" FOR SELECT
                USING (
                    app.user_has_role('Student')
                    AND EXISTS (
                        SELECT 1
                        FROM ""UserCourses""
                        WHERE ""UserCourses"".""CourseId"" = ""Courses"".""CourseId""
                        AND ""UserCourses"".""UserId"" = app.current_user_id()
                    )
                );
            ");

            // Guest: Xem system courses (Type = 1)
            migrationBuilder.Sql(@"
                CREATE POLICY courses_policy_guest_select_system
                ON ""Courses"" FOR SELECT
                USING (
                    ""Type"" = 1
                    AND app.current_user_id() IS NULL
                );
            ");

            // ============================================================================
            // RLS POLICIES CHO USERCOURSES
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_superadmin_all
                ON ""UserCourses"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_admin_all
                ON ""UserCourses"" FOR ALL
                USING (app.user_has_permission('Admin.Course.Manage'));
            ");

            // Teacher: SELECT học sinh trong courses của mình (để xem danh sách)
            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_teacher_select_own_courses
                ON ""UserCourses"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""UserCourses"".""CourseId""
                        AND ""Courses"".""TeacherId"" = app.current_user_id()
                    )
                );
            ");

            // Teacher: INSERT học sinh vào courses của mình
            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_teacher_insert_own_courses
                ON ""UserCourses"" FOR INSERT
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""UserCourses"".""CourseId""
                        AND ""Courses"".""TeacherId"" = app.current_user_id()
                    )
                );
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_teacher_delete_own_courses
                ON ""UserCourses"" FOR DELETE
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""UserCourses"".""CourseId""
                        AND ""Courses"".""TeacherId"" = app.current_user_id()
                    )
                );
            ");

            // Student: Chỉ thao tác trên enrollment của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY usercourses_policy_student_all_own
                ON ""UserCourses"" FOR ALL
                USING (
                    app.user_has_role('Student')
                    AND ""UserId"" = app.current_user_id()
                );
            ");

            // ============================================================================
            // RLS POLICIES CHO LESSONS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_superadmin_all
                ON ""Lessons"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_admin_all
                ON ""Lessons"" FOR ALL
                USING (app.user_has_permission('Admin.Lesson.Manage'));
            ");

            // Teacher: Lessons trong courses của mình (Type = 2)
            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_teacher_select
                ON ""Lessons"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                        AND ""Courses"".""TeacherId"" = app.current_user_id()
                        AND ""Courses"".""Type"" = 2
                    )
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_teacher_insert
                ON ""Lessons"" FOR INSERT
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                        AND ""Courses"".""TeacherId"" = app.current_user_id()
                        AND ""Courses"".""Type"" = 2
                    )
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_teacher_update
                ON ""Lessons"" FOR UPDATE
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                        AND ""Courses"".""TeacherId"" = app.current_user_id()
                        AND ""Courses"".""Type"" = 2
                    )
                )
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                        AND ""Courses"".""TeacherId"" = app.current_user_id()
                        AND ""Courses"".""Type"" = 2
                    )
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_teacher_delete
                ON ""Lessons"" FOR DELETE
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Courses""
                        WHERE ""Courses"".""CourseId"" = ""Lessons"".""CourseId""
                        AND ""Courses"".""TeacherId"" = app.current_user_id()
                        AND ""Courses"".""Type"" = 2
                    )
                );
            ");

            // Student: Xem lessons của courses đã đăng ký
            migrationBuilder.Sql(@"
                CREATE POLICY lessons_policy_student_select_enrolled
                ON ""Lessons"" FOR SELECT
                USING (
                    app.user_has_role('Student')
                    AND EXISTS (
                        SELECT 1
                        FROM ""UserCourses""
                        WHERE ""UserCourses"".""CourseId"" = ""Lessons"".""CourseId""
                        AND ""UserCourses"".""UserId"" = app.current_user_id()
                    )
                );
            ");

            // Guest: KHÔNG có policy - Guest không thể xem lessons (phải đăng ký course trước)

            // ============================================================================
            // RLS POLICIES CHO MODULES
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_superadmin_all
                ON ""Modules"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_admin_all
                ON ""Modules"" FOR ALL
                USING (app.user_has_permission('Admin.Lesson.Manage'));
            ");

            // Teacher: Modules trong courses của mình (Type = 2)
            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_teacher_select
                ON ""Modules"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Lessons"" l
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_teacher_insert
                ON ""Modules"" FOR INSERT
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Lessons"" l
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_teacher_update
                ON ""Modules"" FOR UPDATE
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Lessons"" l
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                )
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Lessons"" l
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                );
            ");
            
            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_teacher_delete
                ON ""Modules"" FOR DELETE
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1 FROM ""Lessons"" l
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                );
            ");

            // Student: Xem modules của lessons đã đăng ký
            migrationBuilder.Sql(@"
                CREATE POLICY modules_policy_student_select_enrolled
                ON ""Modules"" FOR SELECT
                USING (
                    app.user_has_role('Student')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Lessons"" l
                        JOIN ""UserCourses"" uc ON l.""CourseId"" = uc.""CourseId""
                        WHERE l.""LessonId"" = ""Modules"".""LessonId""
                        AND uc.""UserId"" = app.current_user_id()
                    )
                );
            ");

            // Guest: KHÔNG có policy - Guest không thể xem modules (phải đăng ký course trước)

            // ============================================================================
            // RLS POLICIES CHO ESSAYSUBMISSIONS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY essaysubmissions_policy_superadmin_all
                ON ""EssaySubmissions"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY essaysubmissions_policy_admin_all
                ON ""EssaySubmissions"" FOR ALL
                USING (app.user_has_permission('Admin.Lesson.Manage'));
            ");

            // Teacher: Xem essay submissions của students trong courses của mình (chỉ Teacher courses)
            migrationBuilder.Sql(@"
                CREATE POLICY essaysubmissions_policy_teacher_select
                ON ""EssaySubmissions"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Essays"" e
                        JOIN ""Assessments"" a ON e.""AssessmentId"" = a.""AssessmentId""
                        JOIN ""Modules"" m ON a.""ModuleId"" = m.""ModuleId""
                        JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE e.""EssayId"" = ""EssaySubmissions"".""EssayId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                );
            ");

            // Teacher: Cập nhật submissions (chấm điểm, feedback) chỉ trong courses của mình
            migrationBuilder.Sql(@"
                CREATE POLICY essaysubmissions_policy_teacher_update
                ON ""EssaySubmissions"" FOR UPDATE
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Essays"" e
                        JOIN ""Assessments"" a ON e.""AssessmentId"" = a.""AssessmentId""
                        JOIN ""Modules"" m ON a.""ModuleId"" = m.""ModuleId""
                        JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE e.""EssayId"" = ""EssaySubmissions"".""EssayId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                )
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Essays"" e
                        JOIN ""Assessments"" a ON e.""AssessmentId"" = a.""AssessmentId""
                        JOIN ""Modules"" m ON a.""ModuleId"" = m.""ModuleId""
                        JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE e.""EssayId"" = ""EssaySubmissions"".""EssayId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                );
            ");

            // Student: Chỉ thao tác trên submissions của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY essaysubmissions_policy_student_all_own
                ON ""EssaySubmissions"" FOR ALL
                USING (
                    app.user_has_role('Student')
                    AND ""UserId"" = app.current_user_id()
                );
            ");

            // ============================================================================
            // RLS POLICIES CHO QUIZATTEMPTS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY quizattempts_policy_superadmin_all
                ON ""QuizAttempts"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY quizattempts_policy_admin_all
                ON ""QuizAttempts"" FOR ALL
                USING (app.user_has_permission('Admin.Lesson.Manage'));
            ");

            // Teacher: Xem quiz attempts của students trong courses của mình (chỉ Teacher courses)
            migrationBuilder.Sql(@"
                CREATE POLICY quizattempts_policy_teacher_select
                ON ""QuizAttempts"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Quizzes"" q
                        JOIN ""Assessments"" a ON q.""AssessmentId"" = a.""AssessmentId""
                        JOIN ""Modules"" m ON a.""ModuleId"" = m.""ModuleId""
                        JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE q.""QuizId"" = ""QuizAttempts"".""QuizId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                );
            ");

            // Teacher: Cập nhật attempts (force-submit) chỉ trong courses của mình
            migrationBuilder.Sql(@"
                CREATE POLICY quizattempts_policy_teacher_update
                ON ""QuizAttempts"" FOR UPDATE
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Quizzes"" q
                        JOIN ""Assessments"" a ON q.""AssessmentId"" = a.""AssessmentId""
                        JOIN ""Modules"" m ON a.""ModuleId"" = m.""ModuleId""
                        JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE q.""QuizId"" = ""QuizAttempts"".""QuizId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                )
                WITH CHECK (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Quizzes"" q
                        JOIN ""Assessments"" a ON q.""AssessmentId"" = a.""AssessmentId""
                        JOIN ""Modules"" m ON a.""ModuleId"" = m.""ModuleId""
                        JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE q.""QuizId"" = ""QuizAttempts"".""QuizId""
                        AND c.""TeacherId"" = app.current_user_id()
                        AND c.""Type"" = 2
                    )
                );
            ");

            // Student: Chỉ thao tác trên attempts của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY quizattempts_policy_student_all_own
                ON ""QuizAttempts"" FOR ALL
                USING (
                    app.user_has_role('Student')
                    AND ""UserId"" = app.current_user_id()
                );
            ");

            // ============================================================================
            // RLS POLICIES CHO PAYMENTS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_superadmin_all
                ON ""Payments"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_admin_select
                ON ""Payments"" FOR SELECT
                USING (app.user_has_permission('Admin.Payment.Manage'));
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_admin_insert
                ON ""Payments"" FOR INSERT
                WITH CHECK (app.user_has_permission('Admin.Payment.Manage'));
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_admin_update
                ON ""Payments"" FOR UPDATE
                USING (app.user_has_permission('Admin.Payment.Manage'));
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_admin_delete
                ON ""Payments"" FOR DELETE
                USING (app.user_has_permission('Admin.Payment.Manage'));
            ");

            // Student: Chỉ thao tác trên payments của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_student_select_own
                ON ""Payments"" FOR SELECT
                USING (
                    app.user_has_role('Student')
                    AND ""UserId"" = app.current_user_id()
                );
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_student_insert_own
                ON ""Payments"" FOR INSERT
                WITH CHECK (
                    app.user_has_role('Student')
                    AND ""UserId"" = app.current_user_id()
                );
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_student_update_own
                ON ""Payments"" FOR UPDATE
                USING (
                    app.user_has_role('Student')
                    AND ""UserId"" = app.current_user_id()
                );
            ");

            // Webhook (PayOS): Cho phép SELECT và UPDATE khi không có user context (AllowAnonymous)
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_webhook_select
                ON ""Payments"" FOR SELECT
                USING (app.current_user_id() IS NULL);
            ");
            migrationBuilder.Sql(@"
                CREATE POLICY payments_policy_webhook_update
                ON ""Payments"" FOR UPDATE
                USING (app.current_user_id() IS NULL);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all policies
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_superadmin_all ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_admin_select ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_admin_insert ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_admin_update ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_admin_delete ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_teacher_select_update_delete_own ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_teacher_update_own ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_teacher_delete_own ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_teacher_insert_own ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_student_select_system ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_student_select_enrolled ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_guest_select_system ON ""Courses"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS usercourses_policy_superadmin_all ON ""UserCourses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS usercourses_policy_admin_all  ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_teacher_insert ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_teacher_update ON ""Courses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courses_policy_teacher_delete_own_courses ON ""UserCourses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS usercourses_policy_student_all_own ON ""UserCourses"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessons_policy_superadmin_all ON ""Lessons"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessons_policy_admin_all ON ""Lessons"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessons_policy_teacher_select ON ""Lessons"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessons_policy_teacher_insert ON ""Lessons"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessons_policy_teacher_update ON ""Lessons"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessons_policy_teacher_delete ON ""Lessons"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessons_policy_student_select_enrolled ON ""Lessons"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modules_policy_superadmin_all ON ""Modules"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modules_policy_admin_all ON ""Modules"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modules_policy_teacher_select ON ""Modules"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modules_policy_teacher_insert ON ""Modules"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modules_policy_teacher_update ON ""Modules"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modules_policy_teacher_delete ON ""Modules"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modules_policy_student_select_enrolled ON ""Modules"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS essaysubmissions_policy_superadmin_all ON ""EssaySubmissions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS essaysubmissions_policy_admin_all ON ""EssaySubmissions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS essaysubmissions_policy_teacher_select ON ""EssaySubmissions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS essaysubmissions_policy_teacher_update ON ""EssaySubmissions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS essaysubmissions_policy_student_all_own ON ""EssaySubmissions"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS quizattempts_policy_superadmin_all ON ""QuizAttempts"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS quizattempts_policy_admin_all ON ""QuizAttempts"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS quizattempts_policy_teacher_select ON ""QuizAttempts"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS quizattempts_policy_teacher_update ON ""QuizAttempts"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS quizattempts_policy_student_all_own ON ""QuizAttempts"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_superadmin_all ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_admin_select ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_admin_insert ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_admin_update ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_admin_delete ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_student_select_own ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_student_insert_own ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_student_update_own ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_webhook_select ON ""Payments"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS payments_policy_webhook_update ON ""Payments"";");

            // Drop helper functions
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS app.user_has_permission(TEXT);");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS app.is_superadmin();");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS app.user_has_role(TEXT);");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS app.current_user_id();");

            // Drop schema
            migrationBuilder.Sql(@"DROP SCHEMA IF EXISTS app CASCADE;");
        }
    }
}
