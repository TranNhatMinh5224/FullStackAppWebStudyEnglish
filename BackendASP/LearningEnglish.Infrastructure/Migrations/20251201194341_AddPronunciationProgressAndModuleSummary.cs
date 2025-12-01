using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPronunciationProgressAndModuleSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModulePronunciationSummaries",
                columns: table => new
                {
                    ModulePronunciationSummaryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    TotalFlashCards = table.Column<int>(type: "integer", nullable: false),
                    PracticedFlashCards = table.Column<int>(type: "integer", nullable: false),
                    CompletedFlashCards = table.Column<int>(type: "integer", nullable: false),
                    NeedsReviewFlashCards = table.Column<int>(type: "integer", nullable: false),
                    PracticeProgressPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    MasteryPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageModuleScore = table.Column<double>(type: "double precision", nullable: false),
                    HighestScore = table.Column<double>(type: "double precision", nullable: false),
                    LowestScore = table.Column<double>(type: "double precision", nullable: false),
                    TotalAttempts = table.Column<int>(type: "integer", nullable: false),
                    TotalPracticeTimeMinutes = table.Column<float>(type: "real", nullable: false),
                    TopPerformingWordsJson = table.Column<string>(type: "text", nullable: true),
                    StrugglingWordsJson = table.Column<string>(type: "text", nullable: true),
                    ModuleProblemPhonemesJson = table.Column<string>(type: "text", nullable: true),
                    IsModuleCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FirstPracticedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPracticedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "PronunciationProgresses",
                columns: table => new
                {
                    PronunciationProgressId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FlashCardId = table.Column<int>(type: "integer", nullable: false),
                    TotalAttempts = table.Column<int>(type: "integer", nullable: false),
                    PracticeAttempts = table.Column<int>(type: "integer", nullable: false),
                    TestAttempts = table.Column<int>(type: "integer", nullable: false),
                    BestScore = table.Column<double>(type: "double precision", nullable: false),
                    BestAssessmentId = table.Column<int>(type: "integer", nullable: true),
                    BestScoreDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AverageScore = table.Column<double>(type: "double precision", nullable: false),
                    AverageAccuracy = table.Column<double>(type: "double precision", nullable: false),
                    AverageFluency = table.Column<double>(type: "double precision", nullable: false),
                    AverageCompleteness = table.Column<double>(type: "double precision", nullable: false),
                    LatestScore = table.Column<double>(type: "double precision", nullable: false),
                    LastPracticedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LatestAssessmentId = table.Column<int>(type: "integer", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    NeedsReview = table.Column<bool>(type: "boolean", nullable: false),
                    ConsecutiveGoodScores = table.Column<int>(type: "integer", nullable: false),
                    ConsecutivePoorScores = table.Column<int>(type: "integer", nullable: false),
                    CommonProblemPhonemesJson = table.Column<string>(type: "text", nullable: true),
                    ImprovementTipsJson = table.Column<string>(type: "text", nullable: true),
                    TotalPracticeTimeSeconds = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PronunciationProgresses", x => x.PronunciationProgressId);
                    table.ForeignKey(
                        name: "FK_PronunciationProgresses_FlashCards_FlashCardId",
                        column: x => x.FlashCardId,
                        principalTable: "FlashCards",
                        principalColumn: "FlashCardId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PronunciationProgresses_PronunciationAssessments_BestAssess~",
                        column: x => x.BestAssessmentId,
                        principalTable: "PronunciationAssessments",
                        principalColumn: "PronunciationAssessmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PronunciationProgresses_PronunciationAssessments_LatestAsse~",
                        column: x => x.LatestAssessmentId,
                        principalTable: "PronunciationAssessments",
                        principalColumn: "PronunciationAssessmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PronunciationProgresses_Users_UserId",
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
                name: "IX_ModulePronunciationSummaries_ModuleId",
                table: "ModulePronunciationSummaries",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModulePronunciationSummaries_UserId_ModuleId",
                table: "ModulePronunciationSummaries",
                columns: new[] { "UserId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationProgresses_BestAssessmentId",
                table: "PronunciationProgresses",
                column: "BestAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationProgresses_FlashCardId",
                table: "PronunciationProgresses",
                column: "FlashCardId");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationProgresses_LatestAssessmentId",
                table: "PronunciationProgresses",
                column: "LatestAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationProgresses_UserId_FlashCardId",
                table: "PronunciationProgresses",
                columns: new[] { "UserId", "FlashCardId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModulePronunciationSummaries");

            migrationBuilder.DropTable(
                name: "PronunciationProgresses");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$dOoAqc8sM1mNZvzldk9e0uiItI8bEsgVsv09vfdIzaFEEMTGKa2Ne");
        }
    }
}
