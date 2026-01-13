using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.ChemicalDesign.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChemicalLibrary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CanonicalSmiles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    InchiKey = table.Column<string>(type: "nvarchar(27)", maxLength: 27, nullable: true),
                    MolecularFormula = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MolecularWeight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChemicalLibrary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SelectedTargets",
                columns: table => new
                {
                    TargetAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetHypothesisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiseaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GeneSymbol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProteinTarget = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DesiredModulation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LiteratureEvidenceScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    TargetAssessmentScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    ConfidenceLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceUpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SynchronizedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedTargets", x => x.TargetAssessmentId);
                });

            migrationBuilder.CreateTable(
                name: "DesignProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DesiredModulation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignProjects_SelectedTargets_TargetAssessmentId",
                        column: x => x.TargetAssessmentId,
                        principalTable: "SelectedTargets",
                        principalColumn: "TargetAssessmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QueryStructures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Structure = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StructureFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SearchPurpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryStructures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryStructures_DesignProjects_DesignProjectId",
                        column: x => x.DesignProjectId,
                        principalTable: "DesignProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StructureSearchJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueryStructureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SearchType = table.Column<int>(type: "int", nullable: false),
                    SimilarityThreshold = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    MaximumResults = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructureSearchJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StructureSearchJobs_DesignProjects_DesignProjectId",
                        column: x => x.DesignProjectId,
                        principalTable: "DesignProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StructureSearchJobs_QueryStructures_QueryStructureId",
                        column: x => x.QueryStructureId,
                        principalTable: "QueryStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CandidateStructures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StructureSearchJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CanonicalSmiles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    InchiKey = table.Column<string>(type: "nvarchar(27)", maxLength: 27, nullable: true),
                    MolecularFormula = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MolecularWeight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MatchScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SelectionRationale = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SelectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateStructures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateStructures_DesignProjects_DesignProjectId",
                        column: x => x.DesignProjectId,
                        principalTable: "DesignProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateStructures_StructureSearchJobs_StructureSearchJobId",
                        column: x => x.StructureSearchJobId,
                        principalTable: "StructureSearchJobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SynthesisRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateStructureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedYieldPercent = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    StepCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynthesisRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SynthesisRoutes_CandidateStructures_CandidateStructureId",
                        column: x => x.CandidateStructureId,
                        principalTable: "CandidateStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReactionSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SynthesisRouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    ReactionName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Reactants = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Reagents = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Conditions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProductSmiles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EstimatedYieldPercent = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReactionSteps_SynthesisRoutes_SynthesisRouteId",
                        column: x => x.SynthesisRouteId,
                        principalTable: "SynthesisRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateStructures_DesignProjectId",
                table: "CandidateStructures",
                column: "DesignProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateStructures_InchiKey",
                table: "CandidateStructures",
                column: "InchiKey");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateStructures_Status",
                table: "CandidateStructures",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateStructures_StructureSearchJobId",
                table: "CandidateStructures",
                column: "StructureSearchJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalLibrary_InchiKey",
                table: "ChemicalLibrary",
                column: "InchiKey",
                unique: true,
                filter: "[InchiKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DesignProjects_Status",
                table: "DesignProjects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DesignProjects_TargetAssessmentId",
                table: "DesignProjects",
                column: "TargetAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryStructures_DesignProjectId",
                table: "QueryStructures",
                column: "DesignProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionSteps_SynthesisRouteId_StepNumber",
                table: "ReactionSteps",
                columns: new[] { "SynthesisRouteId", "StepNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SelectedTargets_DiseaseName",
                table: "SelectedTargets",
                column: "DiseaseName");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedTargets_GeneSymbol",
                table: "SelectedTargets",
                column: "GeneSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_StructureSearchJobs_DesignProjectId",
                table: "StructureSearchJobs",
                column: "DesignProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StructureSearchJobs_QueryStructureId",
                table: "StructureSearchJobs",
                column: "QueryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_SynthesisRoutes_CandidateStructureId",
                table: "SynthesisRoutes",
                column: "CandidateStructureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChemicalLibrary");

            migrationBuilder.DropTable(
                name: "ReactionSteps");

            migrationBuilder.DropTable(
                name: "SynthesisRoutes");

            migrationBuilder.DropTable(
                name: "CandidateStructures");

            migrationBuilder.DropTable(
                name: "StructureSearchJobs");

            migrationBuilder.DropTable(
                name: "QueryStructures");

            migrationBuilder.DropTable(
                name: "DesignProjects");

            migrationBuilder.DropTable(
                name: "SelectedTargets");
        }
    }
}
