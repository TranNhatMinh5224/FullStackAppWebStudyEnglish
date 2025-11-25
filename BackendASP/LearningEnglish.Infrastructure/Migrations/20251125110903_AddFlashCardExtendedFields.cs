using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashCardExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Antonyms",
                table: "FlashCards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Example",
                table: "FlashCards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExampleTranslation",
                table: "FlashCards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartOfSpeech",
                table: "FlashCards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Synonyms",
                table: "FlashCards",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$7XGFrU2H5SivR76uUe7EAudxBTkoqZY8AdmcHROqMg/eKG59.NMma");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Antonyms",
                table: "FlashCards");

            migrationBuilder.DropColumn(
                name: "Example",
                table: "FlashCards");

            migrationBuilder.DropColumn(
                name: "ExampleTranslation",
                table: "FlashCards");

            migrationBuilder.DropColumn(
                name: "PartOfSpeech",
                table: "FlashCards");

            migrationBuilder.DropColumn(
                name: "Synonyms",
                table: "FlashCards");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$g.k5QXGHpcDRdd.ZV.n0s./9N4ltkLlPh2Id/8UR9Lmu/h/lenJze");
        }
    }
}
