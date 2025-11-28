using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEssaySubmissionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EssaySubmissions_Assessments_AssessmentId",
                table: "EssaySubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_EssaySubmissions_Essays_EssayId",
                table: "EssaySubmissions");

            migrationBuilder.DropIndex(
                name: "IX_EssaySubmissions_AssessmentId",
                table: "EssaySubmissions");

            migrationBuilder.DropColumn(
                name: "AssessmentId",
                table: "EssaySubmissions");

            migrationBuilder.AlterColumn<int>(
                name: "EssayId",
                table: "EssaySubmissions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$d3d7Ivt0ea858VsqTGkZMuoC7EKFE1iThmNeVoncFcUeN61Iq9FqC");

            migrationBuilder.AddForeignKey(
                name: "FK_EssaySubmissions_Essays_EssayId",
                table: "EssaySubmissions",
                column: "EssayId",
                principalTable: "Essays",
                principalColumn: "EssayId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EssaySubmissions_Essays_EssayId",
                table: "EssaySubmissions");

            migrationBuilder.AlterColumn<int>(
                name: "EssayId",
                table: "EssaySubmissions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AssessmentId",
                table: "EssaySubmissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$vaILWwIS4y2wBlk2VwSnR.TpZCsZk6p8/u.fqHD8sSernefdXMBhK");

            migrationBuilder.CreateIndex(
                name: "IX_EssaySubmissions_AssessmentId",
                table: "EssaySubmissions",
                column: "AssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EssaySubmissions_Assessments_AssessmentId",
                table: "EssaySubmissions",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EssaySubmissions_Essays_EssayId",
                table: "EssaySubmissions",
                column: "EssayId",
                principalTable: "Essays",
                principalColumn: "EssayId");
        }
    }
}
