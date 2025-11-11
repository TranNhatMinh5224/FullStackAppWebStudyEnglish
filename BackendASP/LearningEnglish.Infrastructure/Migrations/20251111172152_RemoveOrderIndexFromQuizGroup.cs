using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrderIndexFromQuizGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "QuizSections");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "QuizGroups");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "QuizSections",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$O4hNRF3Awrf1MEDMOpEZLeu3OYQF71Wz8rydnsnud11dvzaAo4.Aa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "QuizSections");

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "Quizzes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "QuizSections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "QuizGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ubTGrOnENRP3e3ttteDxvevv.n/Y1IYRcm8DTufjI7N1mrpmBs/Hm");
        }
    }
}
