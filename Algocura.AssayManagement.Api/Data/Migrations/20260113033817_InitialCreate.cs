using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.AssayManagement.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Screenings",
                columns: table => new
                {
                    ScreeningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ScreeningMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceUpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SynchronizedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screenings", x => x.ScreeningId);
                });

            migrationBuilder.CreateTable(
                name: "Assays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScreeningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssayType = table.Column<int>(type: "int", nullable: false),
                    BiologicalTarget = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Protocol = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MeasurementUnit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActivityThreshold = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QualityControlStatus = table.Column<int>(type: "int", nullable: false),
                    ZPrimeFactor = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    QualityControlNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assays_Screenings_ScreeningId",
                        column: x => x.ScreeningId,
                        principalTable: "Screenings",
                        principalColumn: "ScreeningId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScreeningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Selected = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningResults_Screenings_ScreeningId",
                        column: x => x.ScreeningId,
                        principalTable: "Screenings",
                        principalColumn: "ScreeningId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssayMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasurementType = table.Column<int>(type: "int", nullable: false),
                    Concentration = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ConcentrationUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReplicateNumber = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IsControl = table.Column<bool>(type: "bit", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssayMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssayMeasurements_Assays_AssayId",
                        column: x => x.AssayId,
                        principalTable: "Assays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssayResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeanValue = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    StandardDeviation = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ReplicateCount = table.Column<int>(type: "int", nullable: false),
                    Ic50 = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    Ec50 = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    Ki = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    Kd = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Outcome = table.Column<int>(type: "int", nullable: false),
                    SelectedForAi = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssayResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssayResults_Assays_AssayId",
                        column: x => x.AssayId,
                        principalTable: "Assays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssayMeasurements_AssayId_CompoundId_ReplicateNumber_Concentration",
                table: "AssayMeasurements",
                columns: new[] { "AssayId", "CompoundId", "ReplicateNumber", "Concentration" });

            migrationBuilder.CreateIndex(
                name: "IX_AssayResults_AssayId_CompoundId",
                table: "AssayResults",
                columns: new[] { "AssayId", "CompoundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assays_ScreeningId",
                table: "Assays",
                column: "ScreeningId");

            migrationBuilder.CreateIndex(
                name: "IX_Assays_Status",
                table: "Assays",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningResults_ScreeningId_CompoundId",
                table: "ScreeningResults",
                columns: new[] { "ScreeningId", "CompoundId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssayMeasurements");

            migrationBuilder.DropTable(
                name: "AssayResults");

            migrationBuilder.DropTable(
                name: "ScreeningResults");

            migrationBuilder.DropTable(
                name: "Assays");

            migrationBuilder.DropTable(
                name: "Screenings");
        }
    }
}
