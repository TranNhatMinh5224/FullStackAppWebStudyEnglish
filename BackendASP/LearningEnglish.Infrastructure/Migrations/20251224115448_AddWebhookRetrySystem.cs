using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookRetrySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentWebhookQueues",
                columns: table => new
                {
                    WebhookId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentId = table.Column<int>(type: "integer", nullable: true),
                    OrderCode = table.Column<long>(type: "bigint", nullable: false),
                    WebhookData = table.Column<string>(type: "text", nullable: false),
                    Signature = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentWebhookQueues", x => x.WebhookId);
                    table.ForeignKey(
                        name: "FK_PaymentWebhookQueues_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$d2PfJXjiafZUmLXFY39os.46n.onPhCtL9qkViF/0E/yMGgQShgd2");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookQueues_CreatedAt",
                table: "PaymentWebhookQueues",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookQueues_NextRetryAt",
                table: "PaymentWebhookQueues",
                column: "NextRetryAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookQueues_OrderCode",
                table: "PaymentWebhookQueues",
                column: "OrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookQueues_PaymentId",
                table: "PaymentWebhookQueues",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookQueues_Status",
                table: "PaymentWebhookQueues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookQueues_Status_NextRetryAt",
                table: "PaymentWebhookQueues",
                columns: new[] { "Status", "NextRetryAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentWebhookQueues");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$BDnGmC./3gAofW.xCZxtcO3OkUiX.saIIYgPd4cQJ5YzOlu9mrkPC");
        }
    }
}
