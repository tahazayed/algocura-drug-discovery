using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.Workflow.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "WorkflowStages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "WorkflowStages",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "WorkflowStages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StartedByUserId",
                table: "WorkflowStages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "WorkflowStages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAtUtc",
                table: "Workflows",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "Workflows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "Workflows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Workflows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "Workflows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StartedByUserId",
                table: "Workflows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Workflows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "WorkflowEventReceipts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "WorkflowEventReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCreatedByUserId",
                table: "WorkflowEventReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceUpdatedByUserId",
                table: "WorkflowEventReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "WorkflowEventReceipts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "WorkflowEventReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_CreatedByUserId",
                table: "WorkflowStages",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_UpdatedByUserId",
                table: "WorkflowStages",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_CreatedByUserId",
                table: "Workflows",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_UpdatedByUserId",
                table: "Workflows",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEventReceipts_CreatedByUserId",
                table: "WorkflowEventReceipts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEventReceipts_UpdatedByUserId",
                table: "WorkflowEventReceipts",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkflowStages_CreatedByUserId",
                table: "WorkflowStages");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStages_UpdatedByUserId",
                table: "WorkflowStages");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_CreatedByUserId",
                table: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_UpdatedByUserId",
                table: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowEventReceipts_CreatedByUserId",
                table: "WorkflowEventReceipts");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowEventReceipts_UpdatedByUserId",
                table: "WorkflowEventReceipts");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "StartedByUserId",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "StartedByUserId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "WorkflowEventReceipts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "WorkflowEventReceipts");

            migrationBuilder.DropColumn(
                name: "SourceCreatedByUserId",
                table: "WorkflowEventReceipts");

            migrationBuilder.DropColumn(
                name: "SourceUpdatedByUserId",
                table: "WorkflowEventReceipts");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "WorkflowEventReceipts");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "WorkflowEventReceipts");
        }
    }
}
