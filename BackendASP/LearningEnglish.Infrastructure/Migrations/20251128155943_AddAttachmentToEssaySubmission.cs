using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentToEssaySubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentKey",
                table: "EssaySubmissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentType",
                table: "EssaySubmissions",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$E1aEVyeGW4tNIg6odH4ykO5rb0h5SY9cfwCoR5OmtfdWh7ZvHzi8e");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentKey",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "AttachmentType",
                table: "EssaySubmissions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$chvnvJ/nUaAjZ/zQJ07PP.BYx4Uzm62zc95EmaqR1ujjEGVixrJ36");
        }
    }
}
