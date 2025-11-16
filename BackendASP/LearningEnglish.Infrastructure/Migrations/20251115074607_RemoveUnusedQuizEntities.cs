using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedQuizEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizAttemptResults");

            migrationBuilder.DropTable(
                name: "QuizUserAnswerOptions");

            migrationBuilder.DropTable(
                name: "QuizUserAnswers");

            migrationBuilder.RenameColumn(
                name: "AnswersSnapshot",
                table: "QuizAttempts",
                newName: "ScoresJson");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalScore",
                table: "QuizAttempts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$gJwViVxF4axlFYfZ3otCfOZmh3x6K/pq7OIBXP7xEsixfU8xuM9M2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "QuizAttempts");

            migrationBuilder.RenameColumn(
                name: "ScoresJson",
                table: "QuizAttempts",
                newName: "AnswersSnapshot");

            migrationBuilder.CreateTable(
                name: "QuizAttemptResults",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttemptId = table.Column<int>(type: "integer", nullable: false),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    ManualScore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxScore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<int>(type: "integer", nullable: true),
                    Score = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptResults", x => x.ResultId);
                });

            migrationBuilder.CreateTable(
                name: "QuizUserAnswers",
                columns: table => new
                {
                    QuizUserAnswerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    QuizAttemptId = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AnswerDataJson = table.Column<string>(type: "text", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    MaxPoints = table.Column<decimal>(type: "numeric", nullable: false),
                    PointsEarned = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizUserAnswers", x => x.QuizUserAnswerId);
                    table.ForeignKey(
                        name: "FK_QuizUserAnswers_AnswerOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "AnswerOptionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuizUserAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizUserAnswers_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "AttemptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizUserAnswers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizUserAnswerOptions",
                columns: table => new
                {
                    QuizUserAnswerId = table.Column<int>(type: "integer", nullable: false),
                    AnswerOptionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedOrder = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizUserAnswerOptions", x => new { x.QuizUserAnswerId, x.AnswerOptionId });
                    table.ForeignKey(
                        name: "FK_QuizUserAnswerOptions_AnswerOptions_AnswerOptionId",
                        column: x => x.AnswerOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "AnswerOptionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizUserAnswerOptions_QuizUserAnswers_QuizUserAnswerId",
                        column: x => x.QuizUserAnswerId,
                        principalTable: "QuizUserAnswers",
                        principalColumn: "QuizUserAnswerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$bSP9prsXLW/Yl203BHkmDulABm1tombFo5zhV4MP2eqJgWFf.rr9O");

            migrationBuilder.CreateIndex(
                name: "IX_QuizUserAnswerOptions_AnswerOptionId",
                table: "QuizUserAnswerOptions",
                column: "AnswerOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizUserAnswers_QuestionId",
                table: "QuizUserAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizUserAnswers_QuizAttemptId",
                table: "QuizUserAnswers",
                column: "QuizAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizUserAnswers_SelectedOptionId",
                table: "QuizUserAnswers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizUserAnswers_UserId",
                table: "QuizUserAnswers",
                column: "UserId");
        }
    }
}
