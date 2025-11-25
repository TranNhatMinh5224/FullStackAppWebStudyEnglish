using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPronunciationAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverallScore",
                table: "PronunciationAssessments");

            migrationBuilder.AddColumn<double>(
                name: "AccuracyScore",
                table: "PronunciationAssessments",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "AzureRawResponse",
                table: "PronunciationAssessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CompletenessScore",
                table: "PronunciationAssessments",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "DetailedResultJson",
                table: "PronunciationAssessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "DurationInSeconds",
                table: "PronunciationAssessments",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FluencyScore",
                table: "PronunciationAssessments",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PronunciationScore",
                table: "PronunciationAssessments",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "RecognizedText",
                table: "PronunciationAssessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PronunciationAssessments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PronunciationAssessments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$g.k5QXGHpcDRdd.ZV.n0s./9N4ltkLlPh2Id/8UR9Lmu/h/lenJze");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccuracyScore",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "AzureRawResponse",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "CompletenessScore",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "DetailedResultJson",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "FluencyScore",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "PronunciationScore",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "RecognizedText",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PronunciationAssessments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PronunciationAssessments");

            migrationBuilder.AddColumn<float>(
                name: "OverallScore",
                table: "PronunciationAssessments",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$qE.3sQyI5DgRtZ9gJYDtS.q9DvBbrzL/TkOSCyuRnwvBfKPrpe.Ee");
        }
    }
}
