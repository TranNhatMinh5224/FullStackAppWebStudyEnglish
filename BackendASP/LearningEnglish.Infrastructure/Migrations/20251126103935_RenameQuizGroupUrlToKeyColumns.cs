using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameQuizGroupUrlToKeyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoUrl",
                table: "QuizGroups",
                newName: "VideoKey");

            migrationBuilder.RenameColumn(
                name: "ImgUrl",
                table: "QuizGroups",
                newName: "ImgKey");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$vaILWwIS4y2wBlk2VwSnR.TpZCsZk6p8/u.fqHD8sSernefdXMBhK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoKey",
                table: "QuizGroups",
                newName: "VideoUrl");

            migrationBuilder.RenameColumn(
                name: "ImgKey",
                table: "QuizGroups",
                newName: "ImgUrl");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$VtTFyo5JTTNhIZpLBjsJZODUUtLD7lFz3fnKJ7Fqb0gEJqn6cRrlG");
        }
    }
}
