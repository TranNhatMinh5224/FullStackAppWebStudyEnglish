using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsForUserOwnedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================================
            // ENABLE RLS CHO USER-OWNED TABLES
            // ============================================================================
            
            migrationBuilder.Sql(@"ALTER TABLE ""Notifications"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""Streaks"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""FlashCardReviews"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""CourseProgresses"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""LessonCompletions"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""ModuleCompletions"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""PronunciationProgresses"" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(@"ALTER TABLE ""TeacherSubscriptions"" ENABLE ROW LEVEL SECURITY;");

            // ============================================================================
            // RLS POLICIES CHO NOTIFICATIONS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY notifications_policy_superadmin_all
                ON ""Notifications"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY notifications_policy_admin_all
                ON ""Notifications"" FOR ALL
                USING (app.user_has_permission('Admin.User.Manage'));
            ");

            // User: Chỉ xem/update notifications của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY notifications_policy_user_all_own
                ON ""Notifications"" FOR ALL
                USING (""UserId"" = app.current_user_id());
            ");

            // ============================================================================
            // RLS POLICIES CHO STREAKS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY streaks_policy_superadmin_all
                ON ""Streaks"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY streaks_policy_admin_all
                ON ""Streaks"" FOR ALL
                USING (app.user_has_permission('Admin.User.Manage'));
            ");

            // User: Chỉ xem/update streak của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY streaks_policy_user_all_own
                ON ""Streaks"" FOR ALL
                USING (""UserId"" = app.current_user_id());
            ");

            // ============================================================================
            // RLS POLICIES CHO FLASHCARDREVIEWS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY flashcardreviews_policy_superadmin_all
                ON ""FlashCardReviews"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY flashcardreviews_policy_admin_all
                ON ""FlashCardReviews"" FOR ALL
                USING (app.user_has_permission('Admin.User.Manage'));
            ");

            // User: Chỉ xem/update reviews của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY flashcardreviews_policy_user_all_own
                ON ""FlashCardReviews"" FOR ALL
                USING (""UserId"" = app.current_user_id());
            ");

            // ============================================================================
            // RLS POLICIES CHO COURSEPROGRESSES
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY courseprogresses_policy_superadmin_all
                ON ""CourseProgresses"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY courseprogresses_policy_admin_all
                ON ""CourseProgresses"" FOR ALL
                USING (app.user_has_permission('Admin.User.Manage'));
            ");

            // Teacher: Xem progress của students trong own courses
            migrationBuilder.Sql(@"
                CREATE POLICY courseprogresses_policy_teacher_select_own_courses
                ON ""CourseProgresses"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Courses"" c
                        WHERE c.""CourseId"" = ""CourseProgresses"".""CourseId""
                        AND c.""TeacherId"" = app.current_user_id()
                    )
                );
            ");

            // User: Chỉ xem/update progress của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY courseprogresses_policy_user_all_own
                ON ""CourseProgresses"" FOR ALL
                USING (""UserId"" = app.current_user_id());
            ");

            // ============================================================================
            // RLS POLICIES CHO LESSONCOMPLETIONS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY lessoncompletions_policy_superadmin_all
                ON ""LessonCompletions"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY lessoncompletions_policy_admin_all
                ON ""LessonCompletions"" FOR ALL
                USING (app.user_has_permission('Admin.User.Manage'));
            ");

            // Teacher: Xem completions của students trong own courses
            migrationBuilder.Sql(@"
                CREATE POLICY lessoncompletions_policy_teacher_select_own_courses
                ON ""LessonCompletions"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Lessons"" l
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE l.""LessonId"" = ""LessonCompletions"".""LessonId""
                        AND c.""TeacherId"" = app.current_user_id()
                    )
                );
            ");

            // User: Chỉ xem/update completions của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY lessoncompletions_policy_user_all_own
                ON ""LessonCompletions"" FOR ALL
                USING (""UserId"" = app.current_user_id());
            ");

            // ============================================================================
            // RLS POLICIES CHO MODULECOMPLETIONS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY modulecompletions_policy_superadmin_all
                ON ""ModuleCompletions"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY modulecompletions_policy_admin_all
                ON ""ModuleCompletions"" FOR ALL
                USING (app.user_has_permission('Admin.User.Manage'));
            ");

            // Teacher: Xem completions của students trong own courses
            migrationBuilder.Sql(@"
                CREATE POLICY modulecompletions_policy_teacher_select_own_courses
                ON ""ModuleCompletions"" FOR SELECT
                USING (
                    app.user_has_role('Teacher')
                    AND EXISTS (
                        SELECT 1
                        FROM ""Modules"" m
                        JOIN ""Lessons"" l ON m.""LessonId"" = l.""LessonId""
                        JOIN ""Courses"" c ON l.""CourseId"" = c.""CourseId""
                        WHERE m.""ModuleId"" = ""ModuleCompletions"".""ModuleId""
                        AND c.""TeacherId"" = app.current_user_id()
                    )
                );
            ");

            // User: Chỉ xem/update completions của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY modulecompletions_policy_user_all_own
                ON ""ModuleCompletions"" FOR ALL
                USING (""UserId"" = app.current_user_id());
            ");

            // ============================================================================
            // RLS POLICIES CHO PRONUNCIATIONPROGRESSES
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY pronunciationprogresses_policy_superadmin_all
                ON ""PronunciationProgresses"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY pronunciationprogresses_policy_admin_all
                ON ""PronunciationProgresses"" FOR ALL
                USING (app.user_has_permission('Admin.User.Manage'));
            ");

            // User: Chỉ xem/update progress của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY pronunciationprogresses_policy_user_all_own
                ON ""PronunciationProgresses"" FOR ALL
                USING (""UserId"" = app.current_user_id());
            ");

            // ============================================================================
            // RLS POLICIES CHO TEACHERSUBSCRIPTIONS
            // ============================================================================

            // SuperAdmin: Toàn quyền
            migrationBuilder.Sql(@"
                CREATE POLICY teachersubscriptions_policy_superadmin_all
                ON ""TeacherSubscriptions"" FOR ALL
                USING (app.is_superadmin());
            ");

            // Admin: Permission-aware RLS
            migrationBuilder.Sql(@"
                CREATE POLICY teachersubscriptions_policy_admin_all
                ON ""TeacherSubscriptions"" FOR ALL
                USING (app.user_has_permission('Admin.Package.Manage'));
            ");

            // Teacher: Chỉ xem subscriptions của chính mình
            migrationBuilder.Sql(@"
                CREATE POLICY teachersubscriptions_policy_teacher_all_own
                ON ""TeacherSubscriptions"" FOR ALL
                USING (
                    app.user_has_role('Teacher')
                    AND ""UserId"" = app.current_user_id()
                );
            ");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(556));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(590));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(591));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(593));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(607));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(609));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(610));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(611));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Category", "CreatedAt", "Description", "DisplayName", "Name" },
                values: new object[] { 9, "Finance", new DateTime(2025, 12, 26, 16, 29, 53, 117, DateTimeKind.Utc).AddTicks(588), "Thêm/xóa học viên vào khóa học (dùng khi thanh toán lỗi, nâng cấp user)", "Quản lý học viên trong khóa học", "Admin.Course.Enroll" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "AssignedAt" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                column: "Name",
                value: "ContentAdmin");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                column: "Name",
                value: "FinanceAdmin");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                column: "Name",
                value: "Teacher");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 5, "Student" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$rBop2qbpxhc/zLRyADEO6u55GaDgY31msxNKdNzJtMTGER1WVSntK");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "AssignedAt" },
                values: new object[,]
                {
                    { 9, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop RLS policies
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS notifications_policy_superadmin_all ON ""Notifications"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS notifications_policy_admin_all ON ""Notifications"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS notifications_policy_user_all_own ON ""Notifications"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS streaks_policy_superadmin_all ON ""Streaks"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS streaks_policy_admin_all ON ""Streaks"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS streaks_policy_user_all_own ON ""Streaks"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS flashcardreviews_policy_superadmin_all ON ""FlashCardReviews"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS flashcardreviews_policy_admin_all ON ""FlashCardReviews"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS flashcardreviews_policy_user_all_own ON ""FlashCardReviews"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courseprogresses_policy_superadmin_all ON ""CourseProgresses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courseprogresses_policy_admin_all ON ""CourseProgresses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courseprogresses_policy_teacher_select_own_courses ON ""CourseProgresses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS courseprogresses_policy_user_all_own ON ""CourseProgresses"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessoncompletions_policy_superadmin_all ON ""LessonCompletions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessoncompletions_policy_admin_all ON ""LessonCompletions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessoncompletions_policy_teacher_select_own_courses ON ""LessonCompletions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS lessoncompletions_policy_user_all_own ON ""LessonCompletions"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modulecompletions_policy_superadmin_all ON ""ModuleCompletions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modulecompletions_policy_admin_all ON ""ModuleCompletions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modulecompletions_policy_teacher_select_own_courses ON ""ModuleCompletions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS modulecompletions_policy_user_all_own ON ""ModuleCompletions"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS pronunciationprogresses_policy_superadmin_all ON ""PronunciationProgresses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS pronunciationprogresses_policy_admin_all ON ""PronunciationProgresses"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS pronunciationprogresses_policy_user_all_own ON ""PronunciationProgresses"";");

            migrationBuilder.Sql(@"DROP POLICY IF EXISTS teachersubscriptions_policy_superadmin_all ON ""TeacherSubscriptions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS teachersubscriptions_policy_admin_all ON ""TeacherSubscriptions"";");
            migrationBuilder.Sql(@"DROP POLICY IF EXISTS teachersubscriptions_policy_teacher_all_own ON ""TeacherSubscriptions"";");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 3 });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 4, 48, 8, 519, DateTimeKind.Utc).AddTicks(5320));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 4, 48, 8, 519, DateTimeKind.Utc).AddTicks(5363));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 4, 48, 8, 519, DateTimeKind.Utc).AddTicks(5366));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 4, 48, 8, 519, DateTimeKind.Utc).AddTicks(5368));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 4, 48, 8, 519, DateTimeKind.Utc).AddTicks(5370));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 4, 48, 8, 519, DateTimeKind.Utc).AddTicks(5398));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 4, 48, 8, 519, DateTimeKind.Utc).AddTicks(5431));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 26, 4, 48, 8, 519, DateTimeKind.Utc).AddTicks(5433));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                column: "Name",
                value: "Admin");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                column: "Name",
                value: "Teacher");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                column: "Name",
                value: "Student");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$8CngTpKoUjWU7zYuZLk.luEO0v.AhytBHvux6FuO7uHEwGYSqiJ1.");
        }
    }
}
