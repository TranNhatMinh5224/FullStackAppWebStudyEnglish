using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioAndImageToEssay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioKey",
                table: "Essays",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioType",
                table: "Essays",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageKey",
                table: "Essays",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "Essays",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RYxM4UOpWnQevdZBtHVu.ORLRdqV9PzR4N5gQH7j/ywCYK.eZhg0a");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioKey",
                table: "Essays");

            migrationBuilder.DropColumn(
                name: "AudioType",
                table: "Essays");

            migrationBuilder.DropColumn(
                name: "ImageKey",
                table: "Essays");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "Essays");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ZYuSon/Ll7p0KWl1E61vGOF142eF4adQoWqwO/CBnMDNMETKL90zq");
        }
    }
}
