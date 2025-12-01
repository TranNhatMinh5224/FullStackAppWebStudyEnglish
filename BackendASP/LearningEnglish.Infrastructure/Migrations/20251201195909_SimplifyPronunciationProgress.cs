using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyPronunciationProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PronunciationProgresses_PronunciationAssessments_LatestAsse~",
                table: "PronunciationProgresses");

            migrationBuilder.DropTable(
                name: "ModulePronunciationSummaries");

            migrationBuilder.DropIndex(
                name: "IX_PronunciationProgresses_LatestAssessmentId",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "AverageAccuracy",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "AverageCompleteness",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "AverageFluency",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "AverageScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "CommonProblemPhonemesJson",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "ConsecutiveGoodScores",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "ConsecutivePoorScores",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "ImprovementTipsJson",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "LatestAssessmentId",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "LatestScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "NeedsReview",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "PracticeAttempts",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "TestAttempts",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "TotalPracticeTimeSeconds",
                table: "PronunciationProgresses");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Z3lRLWi0T0iTZUo6.BKAYuxkevfzydYgXnxIFZAPQUvjsHRxFOLCm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageAccuracy",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AverageCompleteness",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AverageFluency",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AverageScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CommonProblemPhonemesJson",
                table: "PronunciationProgresses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveGoodScores",
                table: "PronunciationProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsecutivePoorScores",
                table: "PronunciationProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImprovementTipsJson",
                table: "PronunciationProgresses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "PronunciationProgresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LatestAssessmentId",
                table: "PronunciationProgresses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LatestScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsReview",
                table: "PronunciationProgresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PracticeAttempts",
                table: "PronunciationProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TestAttempts",
                table: "PronunciationProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "TotalPracticeTimeSeconds",
                table: "PronunciationProgresses",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "ModulePronunciationSummaries",
                columns: table => new
                {
                    ModulePronunciationSummaryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AverageModuleScore = table.Column<double>(type: "double precision", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedFlashCards = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstPracticedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HighestScore = table.Column<double>(type: "double precision", nullable: false),
                    IsModuleCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastPracticedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LowestScore = table.Column<double>(type: "double precision", nullable: false),
                    MasteryPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    ModuleProblemPhonemesJson = table.Column<string>(type: "text", nullable: true),
                    NeedsReviewFlashCards = table.Column<int>(type: "integer", nullable: false),
                    PracticeProgressPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    PracticedFlashCards = table.Column<int>(type: "integer", nullable: false),
                    StrugglingWordsJson = table.Column<string>(type: "text", nullable: true),
                    TopPerformingWordsJson = table.Column<string>(type: "text", nullable: true),
                    TotalAttempts = table.Column<int>(type: "integer", nullable: false),
                    TotalFlashCards = table.Column<int>(type: "integer", nullable: false),
                    TotalPracticeTimeMinutes = table.Column<float>(type: "real", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulePronunciationSummaries", x => x.ModulePronunciationSummaryId);
                    table.ForeignKey(
                        name: "FK_ModulePronunciationSummaries_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModulePronunciationSummaries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$cFRvh1jM1WyyBdB0r4eTieh6lnFGapVeHLOsw/XHpAhf0sya.cePi");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationProgresses_LatestAssessmentId",
                table: "PronunciationProgresses",
                column: "LatestAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ModulePronunciationSummaries_ModuleId",
                table: "ModulePronunciationSummaries",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModulePronunciationSummaries_UserId_ModuleId",
                table: "ModulePronunciationSummaries",
                columns: new[] { "UserId", "ModuleId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PronunciationProgresses_PronunciationAssessments_LatestAsse~",
                table: "PronunciationProgresses",
                column: "LatestAssessmentId",
                principalTable: "PronunciationAssessments",
                principalColumn: "PronunciationAssessmentId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
