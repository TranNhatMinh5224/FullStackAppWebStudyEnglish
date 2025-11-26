using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAllUrlToKeyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediaUrl",
                table: "Questions",
                newName: "MediaKey");

            migrationBuilder.RenameColumn(
                name: "AudioUrl",
                table: "PronunciationAssessments",
                newName: "AudioKey");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Lessons",
                newName: "ImageKey");

            migrationBuilder.RenameColumn(
                name: "MediaUrl",
                table: "Lectures",
                newName: "MediaKey");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Courses",
                newName: "ImageKey");

            migrationBuilder.RenameColumn(
                name: "MediaUrl",
                table: "AnswerOptions",
                newName: "MediaKey");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$VtTFyo5JTTNhIZpLBjsJZODUUtLD7lFz3fnKJ7Fqb0gEJqn6cRrlG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediaKey",
                table: "Questions",
                newName: "MediaUrl");

            migrationBuilder.RenameColumn(
                name: "AudioKey",
                table: "PronunciationAssessments",
                newName: "AudioUrl");

            migrationBuilder.RenameColumn(
                name: "ImageKey",
                table: "Lessons",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "MediaKey",
                table: "Lectures",
                newName: "MediaUrl");

            migrationBuilder.RenameColumn(
                name: "ImageKey",
                table: "Courses",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "MediaKey",
                table: "AnswerOptions",
                newName: "MediaUrl");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$8vUl94bBoKNGtKB9WERds.aYrThJA8BMrDFx7kRceyp8e2BgRVL4y");
        }
    }
}
