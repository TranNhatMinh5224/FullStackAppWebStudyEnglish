using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePronunciationAssessmentAssignmentIdToAssessmentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PronunciationAssessments_Assessments_AssignmentId",
                table: "PronunciationAssessments");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "PronunciationAssessments",
                newName: "AssessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_PronunciationAssessments_AssignmentId",
                table: "PronunciationAssessments",
                newName: "IX_PronunciationAssessments_AssessmentId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$dOoAqc8sM1mNZvzldk9e0uiItI8bEsgVsv09vfdIzaFEEMTGKa2Ne");

            migrationBuilder.AddForeignKey(
                name: "FK_PronunciationAssessments_Assessments_AssessmentId",
                table: "PronunciationAssessments",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PronunciationAssessments_Assessments_AssessmentId",
                table: "PronunciationAssessments");

            migrationBuilder.RenameColumn(
                name: "AssessmentId",
                table: "PronunciationAssessments",
                newName: "AssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_PronunciationAssessments_AssessmentId",
                table: "PronunciationAssessments",
                newName: "IX_PronunciationAssessments_AssignmentId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RYxM4UOpWnQevdZBtHVu.ORLRdqV9PzR4N5gQH7j/ywCYK.eZhg0a");

            migrationBuilder.AddForeignKey(
                name: "FK_PronunciationAssessments_Assessments_AssignmentId",
                table: "PronunciationAssessments",
                column: "AssignmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
