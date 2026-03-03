using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.ClinicalResearch.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialClinicalResearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalDatasets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SourceUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StudyCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiseaseArea = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResearchSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalDatasetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalSubjectId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    AgeAtDiagnosis = table.Column<int>(type: "int", nullable: true),
                    Sex = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PrimarySite = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DiseaseStage = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    VitalStatus = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TreatmentSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MolecularFeaturesJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    AdditionalAttributesJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    ImportedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchSubjects_ClinicalDatasets_ClinicalDatasetId",
                        column: x => x.ClinicalDatasetId,
                        principalTable: "ClinicalDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDatasets_Name",
                table: "ClinicalDatasets",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDatasets_Status",
                table: "ClinicalDatasets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDatasets_StudyCode",
                table: "ClinicalDatasets",
                column: "StudyCode");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchSubjects_ClinicalDatasetId_ExternalSubjectId",
                table: "ResearchSubjects",
                columns: new[] { "ClinicalDatasetId", "ExternalSubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResearchSubjects_DiseaseStage",
                table: "ResearchSubjects",
                column: "DiseaseStage");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchSubjects_PrimarySite",
                table: "ResearchSubjects",
                column: "PrimarySite");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResearchSubjects");

            migrationBuilder.DropTable(
                name: "ClinicalDatasets");
        }
    }
}
