using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePronunciationAssessmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PronunciationProgresses_PronunciationAssessments_BestAssess~",
                table: "PronunciationProgresses");

            migrationBuilder.DropTable(
                name: "PronunciationAssessments");

            migrationBuilder.DropIndex(
                name: "IX_PronunciationProgresses_BestAssessmentId",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "BestAssessmentId",
                table: "PronunciationProgresses");

            migrationBuilder.AddColumn<double>(
                name: "AvgAccuracyScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgCompletenessScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgFluencyScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgPronunciationScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveDaysStreak",
                table: "PronunciationProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsMastered",
                table: "PronunciationProgresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "LastAccuracyScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LastFluencyScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LastPronunciationScore",
                table: "PronunciationProgresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStreakDate",
                table: "PronunciationProgresses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MasteredAt",
                table: "PronunciationProgresses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrongPhonemesJson",
                table: "PronunciationProgresses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeakPhonemesJson",
                table: "PronunciationProgresses",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RBElVukuK3s6.KQRVdzn9usCsY6CJNVrBpKoH2BL.cZrDoWTP9SDy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgAccuracyScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "AvgCompletenessScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "AvgFluencyScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "AvgPronunciationScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "ConsecutiveDaysStreak",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "IsMastered",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "LastAccuracyScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "LastFluencyScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "LastPronunciationScore",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "LastStreakDate",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "MasteredAt",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "StrongPhonemesJson",
                table: "PronunciationProgresses");

            migrationBuilder.DropColumn(
                name: "WeakPhonemesJson",
                table: "PronunciationProgresses");

            migrationBuilder.AddColumn<int>(
                name: "BestAssessmentId",
                table: "PronunciationProgresses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PronunciationAssessments",
                columns: table => new
                {
                    PronunciationAssessmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssessmentId = table.Column<int>(type: "integer", nullable: true),
                    FlashCardId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AccuracyScore = table.Column<double>(type: "double precision", nullable: false),
                    AudioKey = table.Column<string>(type: "text", nullable: false),
                    AudioSize = table.Column<long>(type: "bigint", nullable: true),
                    AudioType = table.Column<string>(type: "text", nullable: true),
                    AzureRawResponse = table.Column<string>(type: "text", nullable: true),
                    CompletenessScore = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DetailedResultJson = table.Column<string>(type: "text", nullable: true),
                    DurationInSeconds = table.Column<float>(type: "real", nullable: true),
                    Feedback = table.Column<string>(type: "text", nullable: true),
                    FluencyScore = table.Column<double>(type: "double precision", nullable: false),
                    ProblemPhonemesJson = table.Column<string>(type: "text", nullable: true),
                    PronunciationScore = table.Column<double>(type: "double precision", nullable: false),
                    RecognizedText = table.Column<string>(type: "text", nullable: true),
                    ReferenceText = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StrongPhonemesJson = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WordsDataJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PronunciationAssessments", x => x.PronunciationAssessmentId);
                    table.ForeignKey(
                        name: "FK_PronunciationAssessments_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PronunciationAssessments_FlashCards_FlashCardId",
                        column: x => x.FlashCardId,
                        principalTable: "FlashCards",
                        principalColumn: "FlashCardId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PronunciationAssessments_Users_UserId",
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
                value: "$2a$11$Z3lRLWi0T0iTZUo6.BKAYuxkevfzydYgXnxIFZAPQUvjsHRxFOLCm");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationProgresses_BestAssessmentId",
                table: "PronunciationProgresses",
                column: "BestAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationAssessments_AssessmentId",
                table: "PronunciationAssessments",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationAssessments_FlashCardId",
                table: "PronunciationAssessments",
                column: "FlashCardId");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationAssessments_UserId",
                table: "PronunciationAssessments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PronunciationProgresses_PronunciationAssessments_BestAssess~",
                table: "PronunciationProgresses",
                column: "BestAssessmentId",
                principalTable: "PronunciationAssessments",
                principalColumn: "PronunciationAssessmentId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
