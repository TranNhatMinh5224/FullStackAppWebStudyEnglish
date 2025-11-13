using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMediaAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Users_ReviewedBy",
                table: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizGroupMediaAssets");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "MaxScore",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "ShuffleSeedJson",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "TeacherFeedback",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "AnswerOptions");

            migrationBuilder.RenameColumn(
                name: "ReviewedBy",
                table: "QuizAttempts",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_QuizAttempts_ReviewedBy",
                table: "QuizAttempts",
                newName: "IX_QuizAttempts_UserId1");

            migrationBuilder.AddColumn<string>(
                name: "ImgUrl",
                table: "QuizGroups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "QuizGroups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "QuizGroups",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuizAttemptResults",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttemptId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ManualScore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true),
                    ReviewedBy = table.Column<int>(type: "integer", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptResults", x => x.ResultId);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$5K3Fs8diVzt3mxwuwCeStOBgYppkgfyLKK.vZ9TFH4qucjII9NUmG");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_UserId1",
                table: "QuizAttempts",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Users_UserId1",
                table: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizAttemptResults");

            migrationBuilder.DropColumn(
                name: "ImgUrl",
                table: "QuizGroups");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "QuizGroups");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "QuizGroups");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "QuizAttempts",
                newName: "ReviewedBy");

            migrationBuilder.RenameIndex(
                name: "IX_QuizAttempts_UserId1",
                table: "QuizAttempts",
                newName: "IX_QuizAttempts_ReviewedBy");

            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "QuizAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxScore",
                table: "QuizAttempts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                table: "QuizAttempts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "QuizAttempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "QuizAttempts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShuffleSeedJson",
                table: "QuizAttempts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherFeedback",
                table: "QuizAttempts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "AnswerOptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    MediaAssetId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.MediaAssetId);
                });

            migrationBuilder.CreateTable(
                name: "QuizGroupMediaAssets",
                columns: table => new
                {
                    MediaAssetId = table.Column<int>(type: "integer", nullable: false),
                    QuizGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizGroupMediaAssets", x => new { x.MediaAssetId, x.QuizGroupId });
                    table.ForeignKey(
                        name: "FK_QuizGroupMediaAssets_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "MediaAssetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizGroupMediaAssets_QuizGroups_QuizGroupId",
                        column: x => x.QuizGroupId,
                        principalTable: "QuizGroups",
                        principalColumn: "QuizGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ZP/HBJSsNaaNg5SjLF82AOceTB0jEP66XzcteOkzE2eWSEVUiL1Be");

            migrationBuilder.CreateIndex(
                name: "IX_QuizGroupMediaAssets_QuizGroupId",
                table: "QuizGroupMediaAssets",
                column: "QuizGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_ReviewedBy",
                table: "QuizAttempts",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
