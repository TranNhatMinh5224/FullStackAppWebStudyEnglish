using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUsedToEmailVerificationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockedUntil",
                table: "EmailVerificationTokens");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$I4RofovM/yU9Sbvx6SeZMOfD66upIQH07Hy.wcbKd4/8kwK/zLidO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BlockedUntil",
                table: "EmailVerificationTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$faUkQHMBjqFPM6lNc5c5TOgvTHMONnH2hhotJvxY8mYEVlutiwE9a");
        }
    }
}
