using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBusService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialEventBusSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WebhookUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    LastDeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EventPayload = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    Topic = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PartitionKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PublishAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSubscription_EventType",
                table: "EventSubscriptions",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_EventSubscription_IsActive",
                table: "EventSubscriptions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EventSubscription_ServiceName",
                table: "EventSubscriptions",
                column: "ServiceName");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvent_CreatedAt",
                table: "OutboxEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvent_EventType",
                table: "OutboxEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvent_IsPublished",
                table: "OutboxEvents",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSubscriptions");

            migrationBuilder.DropTable(
                name: "OutboxEvents");
        }
    }
}
