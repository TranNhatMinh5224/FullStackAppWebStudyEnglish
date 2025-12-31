using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModuleTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4575));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4596));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4597));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4598));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4606));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4607));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4608));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4609));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 15, 48, 50, 903, DateTimeKind.Utc).AddTicks(4594));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$4RNl5J8xV68I0pktCBX/r.bBqyez3S9vlj2yvvXS7Tu.kl4i2edXK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1696));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1712));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1714));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1715));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1721));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1722));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1723));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1724));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 23, 3, 0, 900, DateTimeKind.Utc).AddTicks(1711));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$2FuqXJMADpSoyPIdqOJKPu8QsXVK70zRVMUyd2jQ.Ad3g8tNfTuyy");
        }
    }
}
