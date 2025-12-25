using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Category", "CreatedAt", "Description", "DisplayName", "Name" },
                values: new object[,]
                {
                    { 1, "Content", new DateTime(2025, 12, 25, 8, 40, 10, 133, DateTimeKind.Utc).AddTicks(8015), "Tạo, sửa, xóa, publish khóa học", "Quản lý khóa học", "Admin.Course.Manage" },
                    { 2, "Content", new DateTime(2025, 12, 25, 8, 40, 10, 133, DateTimeKind.Utc).AddTicks(8047), "Tạo, sửa, xóa lessons và modules", "Quản lý bài học", "Admin.Lesson.Manage" },
                    { 3, "Content", new DateTime(2025, 12, 25, 8, 40, 10, 133, DateTimeKind.Utc).AddTicks(8053), "Quản lý flashcards, quizzes, essays, assets frontend", "Quản lý nội dung", "Admin.Content.Manage" },
                    { 4, "Finance", new DateTime(2025, 12, 25, 8, 40, 10, 133, DateTimeKind.Utc).AddTicks(8057), "Xem, block/unblock, xóa users, gán roles", "Quản lý người dùng", "Admin.User.Manage" },
                    { 5, "Finance", new DateTime(2025, 12, 25, 8, 40, 10, 133, DateTimeKind.Utc).AddTicks(8061), "Xem payments, hoàn tiền, fix lỗi thanh toán", "Quản lý thanh toán", "Admin.Payment.Manage" },
                    { 6, "Finance", new DateTime(2025, 12, 25, 8, 40, 10, 133, DateTimeKind.Utc).AddTicks(8077), "Xem báo cáo doanh thu và thống kê tài chính", "Xem doanh thu", "Admin.Revenue.View" },
                    { 7, "Finance", new DateTime(2025, 12, 25, 8, 40, 10, 133, DateTimeKind.Utc).AddTicks(8114), "Tạo, sửa, xóa teacher packages", "Quản lý gói giáo viên", "Admin.Package.Manage" },
                    { 8, "System", new DateTime(2025, 12, 25, 8, 40, 10, 133, DateTimeKind.Utc).AddTicks(8118), "Super Admin - full permissions, không thể thu hồi", "Toàn quyền hệ thống", "Admin.System.FullAccess" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$AR7L/omHs85/ZgrMRpD7vO7ywYJ22Xt0HTA0galJl2Nr2IT/EDueu");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "AssignedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$bXcFefeytDYiCkKwZ2GfR.Hj/ITRLjclVvFGmLHjL8rtjpHofHnt2");
        }
    }
}
