using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAvatarTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarType",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Epg2fvvbQdzSQUE7YA2FSeBoxojMeFwMg/GljYKwj6r/cTtB8K6Ym");
        }

        private static readonly string[] columns = new[] { "AvatarType", "PasswordHash" };

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarType",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: columns,
                values: new object[] { null, "$2a$11$41kJfaKF1HhxbIwtHts/X.890QjNtyhXK.D0fgWUv/LekrVSbvwzG" });
        }
    }
}
