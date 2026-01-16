using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.ChemicalDesign.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAtUtc",
                table: "SynthesisRoutes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "SynthesisRoutes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "SynthesisRoutes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "SynthesisRoutes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "StructureSearchJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "StructureSearchJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "StructureSearchJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "SelectedTargets",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "SelectedTargets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCreatedByUserId",
                table: "SelectedTargets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceUpdatedByUserId",
                table: "SelectedTargets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "SelectedTargets",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "SelectedTargets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "ReactionSteps",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ReactionSteps",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "ReactionSteps",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ReactionSteps",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "QueryStructures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "QueryStructures",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "QueryStructures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "DesignProjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "DesignProjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "DesignProjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "DesignProjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "ChemicalLibrary",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ChemicalLibrary",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "ChemicalLibrary",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ChemicalLibrary",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "CandidateStructures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedByUserId",
                table: "CandidateStructures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "CandidateStructures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SynthesisRoutes_CreatedByUserId",
                table: "SynthesisRoutes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SynthesisRoutes_UpdatedByUserId",
                table: "SynthesisRoutes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StructureSearchJobs_CreatedByUserId",
                table: "StructureSearchJobs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StructureSearchJobs_UpdatedByUserId",
                table: "StructureSearchJobs",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedTargets_CreatedByUserId",
                table: "SelectedTargets",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedTargets_UpdatedByUserId",
                table: "SelectedTargets",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionSteps_CreatedByUserId",
                table: "ReactionSteps",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionSteps_UpdatedByUserId",
                table: "ReactionSteps",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryStructures_CreatedByUserId",
                table: "QueryStructures",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryStructures_UpdatedByUserId",
                table: "QueryStructures",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignProjects_CreatedByUserId",
                table: "DesignProjects",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignProjects_UpdatedByUserId",
                table: "DesignProjects",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalLibrary_CreatedByUserId",
                table: "ChemicalLibrary",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalLibrary_UpdatedByUserId",
                table: "ChemicalLibrary",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateStructures_CreatedByUserId",
                table: "CandidateStructures",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateStructures_UpdatedByUserId",
                table: "CandidateStructures",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SynthesisRoutes_CreatedByUserId",
                table: "SynthesisRoutes");

            migrationBuilder.DropIndex(
                name: "IX_SynthesisRoutes_UpdatedByUserId",
                table: "SynthesisRoutes");

            migrationBuilder.DropIndex(
                name: "IX_StructureSearchJobs_CreatedByUserId",
                table: "StructureSearchJobs");

            migrationBuilder.DropIndex(
                name: "IX_StructureSearchJobs_UpdatedByUserId",
                table: "StructureSearchJobs");

            migrationBuilder.DropIndex(
                name: "IX_SelectedTargets_CreatedByUserId",
                table: "SelectedTargets");

            migrationBuilder.DropIndex(
                name: "IX_SelectedTargets_UpdatedByUserId",
                table: "SelectedTargets");

            migrationBuilder.DropIndex(
                name: "IX_ReactionSteps_CreatedByUserId",
                table: "ReactionSteps");

            migrationBuilder.DropIndex(
                name: "IX_ReactionSteps_UpdatedByUserId",
                table: "ReactionSteps");

            migrationBuilder.DropIndex(
                name: "IX_QueryStructures_CreatedByUserId",
                table: "QueryStructures");

            migrationBuilder.DropIndex(
                name: "IX_QueryStructures_UpdatedByUserId",
                table: "QueryStructures");

            migrationBuilder.DropIndex(
                name: "IX_DesignProjects_CreatedByUserId",
                table: "DesignProjects");

            migrationBuilder.DropIndex(
                name: "IX_DesignProjects_UpdatedByUserId",
                table: "DesignProjects");

            migrationBuilder.DropIndex(
                name: "IX_ChemicalLibrary_CreatedByUserId",
                table: "ChemicalLibrary");

            migrationBuilder.DropIndex(
                name: "IX_ChemicalLibrary_UpdatedByUserId",
                table: "ChemicalLibrary");

            migrationBuilder.DropIndex(
                name: "IX_CandidateStructures_CreatedByUserId",
                table: "CandidateStructures");

            migrationBuilder.DropIndex(
                name: "IX_CandidateStructures_UpdatedByUserId",
                table: "CandidateStructures");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "SynthesisRoutes");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "SynthesisRoutes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SynthesisRoutes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "SynthesisRoutes");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "StructureSearchJobs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "StructureSearchJobs");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "StructureSearchJobs");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "SelectedTargets");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SelectedTargets");

            migrationBuilder.DropColumn(
                name: "SourceCreatedByUserId",
                table: "SelectedTargets");

            migrationBuilder.DropColumn(
                name: "SourceUpdatedByUserId",
                table: "SelectedTargets");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "SelectedTargets");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "SelectedTargets");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ReactionSteps");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ReactionSteps");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ReactionSteps");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ReactionSteps");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "QueryStructures");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "QueryStructures");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "QueryStructures");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "DesignProjects");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "DesignProjects");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "DesignProjects");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "DesignProjects");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ChemicalLibrary");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ChemicalLibrary");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ChemicalLibrary");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ChemicalLibrary");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CandidateStructures");

            migrationBuilder.DropColumn(
                name: "SelectedByUserId",
                table: "CandidateStructures");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "CandidateStructures");
        }
    }
}
