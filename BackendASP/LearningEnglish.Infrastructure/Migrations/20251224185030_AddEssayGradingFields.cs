using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEssayGradingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "EssaySubmissions",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GradedAt",
                table: "EssaySubmissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GradedByTeacherId",
                table: "EssaySubmissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "EssaySubmissions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherFeedback",
                table: "EssaySubmissions",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeacherGradedAt",
                table: "EssaySubmissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TeacherScore",
                table: "EssaySubmissions",
                type: "numeric",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$.4dLtMVJb.hdW9QPIreIweLOXRqPxytH66GbN/VyL.BQ5t54K1aJi");

            migrationBuilder.CreateIndex(
                name: "IX_EssaySubmissions_GradedByTeacherId",
                table: "EssaySubmissions",
                column: "GradedByTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_EssaySubmissions_Users_GradedByTeacherId",
                table: "EssaySubmissions",
                column: "GradedByTeacherId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EssaySubmissions_Users_GradedByTeacherId",
                table: "EssaySubmissions");

            migrationBuilder.DropIndex(
                name: "IX_EssaySubmissions_GradedByTeacherId",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "GradedAt",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "GradedByTeacherId",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "TeacherFeedback",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "TeacherGradedAt",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "TeacherScore",
                table: "EssaySubmissions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ueS442CBdAjFIA9alzuQG.P9GzempV5YzQsK1AZUmfSYjMRXPktO.");
        }
    }
}
