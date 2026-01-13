using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.LiteratureIntelligence.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Abstract = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Journal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublishedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AnalyzedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TargetHypotheses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiseaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GeneSymbol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProteinTarget = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DesiredModulation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetHypotheses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvidenceStatements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiseaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GeneSymbol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProteinTarget = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RelationshipType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Statement = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    HumanValidated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenceStatements_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationAuthors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationAuthors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationAuthors_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_TargetHypothesisEvidence_EvidenceStatements_EvidenceStatementId",
                        column: x => x.EvidenceStatementId,
                        principalTable: "EvidenceStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TargetHypothesisEvidence_TargetHypotheses_TargetHypothesisId",
                        column: x => x.TargetHypothesisId,
                        principalTable: "TargetHypotheses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceStatements_DiseaseName",
                table: "EvidenceStatements",
                column: "DiseaseName");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceStatements_GeneSymbol",
                table: "EvidenceStatements",
                column: "GeneSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceStatements_PublicationId",
                table: "EvidenceStatements",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationAuthors_PublicationId_DisplayOrder",
                table: "PublicationAuthors",
                columns: new[] { "PublicationId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ExternalId",
                table: "Publications",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TargetHypothesisEvidence_EvidenceStatementId",
                table: "TargetHypothesisEvidence",
                column: "EvidenceStatementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicationAuthors");

            migrationBuilder.DropTable(
                name: "TargetHypothesisEvidence");

            migrationBuilder.DropTable(
                name: "EvidenceStatements");

            migrationBuilder.DropTable(
                name: "TargetHypotheses");

            migrationBuilder.DropTable(
                name: "Publications");
        }
    }
}
