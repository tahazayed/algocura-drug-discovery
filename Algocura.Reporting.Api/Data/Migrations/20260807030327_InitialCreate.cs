using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.Reporting.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReportType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultParametersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportingProjections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectionType = table.Column<int>(type: "int", nullable: false),
                    SourceEventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceService = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DiseaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GeneSymbol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompoundName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Score = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    WorkflowId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceUpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SynchronizedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SourceCreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceUpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingProjections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResultLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportRuns_ReportDefinitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportMetrics_ReportRuns_ReportRunId",
                        column: x => x.ReportRunId,
                        principalTable: "ReportRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_CreatedByUserId",
                table: "ReportDefinitions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_Name",
                table: "ReportDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_Status",
                table: "ReportDefinitions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_UpdatedByUserId",
                table: "ReportDefinitions",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProjections_CompoundName",
                table: "ReportingProjections",
                column: "CompoundName");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProjections_CreatedByUserId",
                table: "ReportingProjections",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProjections_DiseaseName",
                table: "ReportingProjections",
                column: "DiseaseName");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProjections_GeneSymbol",
                table: "ReportingProjections",
                column: "GeneSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProjections_ProjectionType",
                table: "ReportingProjections",
                column: "ProjectionType");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProjections_SourceService_SourceEntityId_ProjectionType",
                table: "ReportingProjections",
                columns: new[] { "SourceService", "SourceEntityId", "ProjectionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProjections_UpdatedByUserId",
                table: "ReportingProjections",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProjections_WorkflowId",
                table: "ReportingProjections",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportMetrics_CreatedByUserId",
                table: "ReportMetrics",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportMetrics_ReportRunId_Name",
                table: "ReportMetrics",
                columns: new[] { "ReportRunId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportMetrics_UpdatedByUserId",
                table: "ReportMetrics",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_CreatedByUserId",
                table: "ReportRuns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_ReportDefinitionId",
                table: "ReportRuns",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_Status",
                table: "ReportRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_UpdatedByUserId",
                table: "ReportRuns",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportingProjections");

            migrationBuilder.DropTable(
                name: "ReportMetrics");

            migrationBuilder.DropTable(
                name: "ReportRuns");

            migrationBuilder.DropTable(
                name: "ReportDefinitions");
        }
    }
}
