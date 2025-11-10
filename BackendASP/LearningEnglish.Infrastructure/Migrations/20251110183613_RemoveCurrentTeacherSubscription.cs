using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCurrentTeacherSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_TeacherSubscriptions_CurrentTeacherSubscriptionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CurrentTeacherSubscriptionId",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "CurrentTeacherSubscriptionTeacherSubscriptionId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CurrentTeacherSubscriptionTeacherSubscriptionId", "PasswordHash" },
                values: new object[] { null, "$2a$11$qtVZzqYtzD5/FFSi08zP8e.OtnDU0TZW02BWonuONhls/oyYdA/BO" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentTeacherSubscriptionTeacherSubscriptionId",
                table: "Users",
                column: "CurrentTeacherSubscriptionTeacherSubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TeacherSubscriptions_CurrentTeacherSubscriptionTeache~",
                table: "Users",
                column: "CurrentTeacherSubscriptionTeacherSubscriptionId",
                principalTable: "TeacherSubscriptions",
                principalColumn: "TeacherSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_TeacherSubscriptions_CurrentTeacherSubscriptionTeache~",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CurrentTeacherSubscriptionTeacherSubscriptionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrentTeacherSubscriptionTeacherSubscriptionId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$XYeQqRWesKRFhKFDw3gZre/dpsyjUkBfFC8Zg8PPkEsZtuhtaWTJi");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentTeacherSubscriptionId",
                table: "Users",
                column: "CurrentTeacherSubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TeacherSubscriptions_CurrentTeacherSubscriptionId",
                table: "Users",
                column: "CurrentTeacherSubscriptionId",
                principalTable: "TeacherSubscriptions",
                principalColumn: "TeacherSubscriptionId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
