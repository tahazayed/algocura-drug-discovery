using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.Notification.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAtUtc",
                table: "Notifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCreatedByUserId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceUpdatedByUserId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "NotificationRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "NotificationRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "NotificationRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "NotificationDeliveryAttempts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "NotificationDeliveryAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "NotificationDeliveryAttempts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "NotificationDeliveryAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAtUtc",
                table: "NotificationDeliveries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "NotificationDeliveries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "NotificationDeliveries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "NotificationDeliveries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedByUserId",
                table: "Notifications",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UpdatedByUserId",
                table: "Notifications",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_CreatedByUserId",
                table: "NotificationRules",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_UpdatedByUserId",
                table: "NotificationRules",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveryAttempts_CreatedByUserId",
                table: "NotificationDeliveryAttempts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveryAttempts_UpdatedByUserId",
                table: "NotificationDeliveryAttempts",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_CreatedByUserId",
                table: "NotificationDeliveries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_UpdatedByUserId",
                table: "NotificationDeliveries",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedByUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UpdatedByUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRules_CreatedByUserId",
                table: "NotificationRules");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRules_UpdatedByUserId",
                table: "NotificationRules");

            migrationBuilder.DropIndex(
                name: "IX_NotificationDeliveryAttempts_CreatedByUserId",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropIndex(
                name: "IX_NotificationDeliveryAttempts_UpdatedByUserId",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropIndex(
                name: "IX_NotificationDeliveries_CreatedByUserId",
                table: "NotificationDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_NotificationDeliveries_UpdatedByUserId",
                table: "NotificationDeliveries");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SourceCreatedByUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SourceUpdatedByUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "NotificationDeliveries");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "NotificationDeliveries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "NotificationDeliveries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "NotificationDeliveries");
        }
    }
}
