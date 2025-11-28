using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWordLevelDataToPronunciationAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProblemPhonemesJson",
                table: "PronunciationAssessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrongPhonemesJson",
                table: "PronunciationAssessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WordsDataJson",
                table: "PronunciationAssessments",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ZYuSon/Ll7p0KWl1E61vGOF142eF4adQoWqwO/CBnMDNMETKL90zq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProblemPhonemesJson",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "StrongPhonemesJson",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "WordsDataJson",
                table: "PronunciationAssessments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$E1aEVyeGW4tNIg6odH4ykO5rb0h5SY9cfwCoR5OmtfdWh7ZvHzi8e");
        }
    }
}
