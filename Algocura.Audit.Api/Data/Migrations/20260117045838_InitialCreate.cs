using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.Audit.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditRecords",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserDisplayName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    ActorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PreviousValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedPropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WorkflowId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditRecords", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_CorrelationId",
                table: "AuditRecords",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_CreatedByUserId",
                table: "AuditRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_EntityType_EntityId",
                table: "AuditRecords",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_OccurredAtUtc",
                table: "AuditRecords",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_ServiceName",
                table: "AuditRecords",
                column: "ServiceName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_UpdatedByUserId",
                table: "AuditRecords",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_UserId",
                table: "AuditRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_WorkflowId",
                table: "AuditRecords",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditRecords");
        }
    }
}
