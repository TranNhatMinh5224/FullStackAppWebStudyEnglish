using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizTimingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuizGroups_QuizGroupId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuizSections_QuizSectionId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ShowCorrectAnswersDuringAttempt",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Quizzes",
                newName: "AvailableFrom");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Quizzes",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$bSP9prsXLW/Yl203BHkmDulABm1tombFo5zhV4MP2eqJgWFf.rr9O");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuizGroups_QuizGroupId",
                table: "Questions",
                column: "QuizGroupId",
                principalTable: "QuizGroups",
                principalColumn: "QuizGroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuizSections_QuizSectionId",
                table: "Questions",
                column: "QuizSectionId",
                principalTable: "QuizSections",
                principalColumn: "QuizSectionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuizGroups_QuizGroupId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuizSections_QuizSectionId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "AvailableFrom",
                table: "Quizzes",
                newName: "StartTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Quizzes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCorrectAnswersDuringAttempt",
                table: "Quizzes",
                type: "boolean",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$AtRcrt2q0pTC8LtenmbwC.PaSuXLJYdhPTSyaqAPDQY.bcyeVN/vm");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuizGroups_QuizGroupId",
                table: "Questions",
                column: "QuizGroupId",
                principalTable: "QuizGroups",
                principalColumn: "QuizGroupId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuizSections_QuizSectionId",
                table: "Questions",
                column: "QuizSectionId",
                principalTable: "QuizSections",
                principalColumn: "QuizSectionId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
