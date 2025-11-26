using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameFlashCardUrlToKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "FlashCards",
                newName: "ImageKey");

            migrationBuilder.RenameColumn(
                name: "AudioUrl",
                table: "FlashCards",
                newName: "AudioKey");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$8vUl94bBoKNGtKB9WERds.aYrThJA8BMrDFx7kRceyp8e2BgRVL4y");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageKey",
                table: "FlashCards",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "AudioKey",
                table: "FlashCards",
                newName: "AudioUrl");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Q0vzuLX3A0IF67Zd9TtiFefmb79i7tNZg87eOC8wvMGRsogmZU1.G");
        }
    }
}
