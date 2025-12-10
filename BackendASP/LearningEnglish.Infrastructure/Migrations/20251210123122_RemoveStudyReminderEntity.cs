using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudyReminderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudyReminders");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "DateOfBirth", "EmailVerified", "NormalizedEmail", "PasswordHash" },
                values: new object[] { new DateTime(2004, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, "MINHXOANDEV@GMAIL.COM", "$2a$11$faUkQHMBjqFPM6lNc5c5TOgvTHMONnH2hhotJvxY8mYEVlutiwE9a" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudyReminders",
                columns: table => new
                {
                    StudyReminderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmailEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsPushEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NextScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SentCount = table.Column<int>(type: "integer", nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyReminders", x => x.StudyReminderId);
                    table.ForeignKey(
                        name: "FK_StudyReminders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "DateOfBirth", "EmailVerified", "NormalizedEmail", "PasswordHash" },
                values: new object[] { null, false, "", "$2a$11$Epg2fvvbQdzSQUE7YA2FSeBoxojMeFwMg/GljYKwj6r/cTtB8K6Ym" });

            migrationBuilder.CreateIndex(
                name: "IX_StudyReminders_UserId",
                table: "StudyReminders",
                column: "UserId");
        }
    }
}
