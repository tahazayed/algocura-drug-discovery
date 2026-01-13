using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.TargetDiscovery.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TargetHypotheses",
                columns: table => new
                {
                    TargetHypothesisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiseaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GeneSymbol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProteinTarget = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DesiredModulation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LiteratureEvidenceScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceUpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SynchronizedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetHypotheses", x => x.TargetHypothesisId);
                });

            migrationBuilder.CreateTable(
                name: "TargetAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetHypothesisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ScientificRationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LiteratureEvidenceScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    GeneticEvidenceScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    BiologicalPlausibilityScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    DruggabilityScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    SafetyRiskScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    TranslationalPotentialScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    OverallScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    ConfidenceLevel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SelectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetAssessments_TargetHypotheses_TargetHypothesisId",
                        column: x => x.TargetHypothesisId,
                        principalTable: "TargetHypotheses",
                        principalColumn: "TargetHypothesisId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TargetHypothesisEvidence",
                columns: table => new
                {
                    TargetHypothesisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvidenceStatementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetHypothesisEvidence", x => new { x.TargetHypothesisId, x.EvidenceStatementId });
                    table.ForeignKey(
                        name: "FK_TargetHypothesisEvidence_TargetHypotheses_TargetHypothesisId",
                        column: x => x.TargetHypothesisId,
                        principalTable: "TargetHypotheses",
                        principalColumn: "TargetHypothesisId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TargetAssessments_Status",
                table: "TargetAssessments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TargetAssessments_TargetHypothesisId",
                table: "TargetAssessments",
                column: "TargetHypothesisId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetHypotheses_DiseaseName",
                table: "TargetHypotheses",
                column: "DiseaseName");

            migrationBuilder.CreateIndex(
                name: "IX_TargetHypotheses_GeneSymbol",
                table: "TargetHypotheses",
                column: "GeneSymbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetAssessments");

            migrationBuilder.DropTable(
                name: "TargetHypothesisEvidence");

            migrationBuilder.DropTable(
                name: "TargetHypotheses");
        }
    }
}
