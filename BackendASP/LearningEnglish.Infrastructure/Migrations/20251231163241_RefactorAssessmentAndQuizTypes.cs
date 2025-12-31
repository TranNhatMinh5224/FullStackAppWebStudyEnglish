using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAssessmentAndQuizTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Essays");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "Assessments");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPoints",
                table: "Essays",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1199));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1220));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1221));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1223));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1232));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1233));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1235));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1236));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 16, 32, 41, 236, DateTimeKind.Utc).AddTicks(1218));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$OpUTFXDBiHSL9fm23ywe3u0q3Ld6nZMVF/SB5SBuPMqnX6Y4VrcE.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "Essays");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Essays",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PassingScore",
                table: "Assessments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPoints",
                table: "Assessments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

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
    }
}
