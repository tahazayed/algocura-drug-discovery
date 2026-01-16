using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.AIDiscovery.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "Screenings",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Screenings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCreatedByUserId",
                table: "Screenings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceUpdatedByUserId",
                table: "Screenings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Screenings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "ScreeningResults",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ScreeningResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCreatedByUserId",
                table: "ScreeningResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceUpdatedByUserId",
                table: "ScreeningResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "ScreeningResults",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ScreeningResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAtUtc",
                table: "PredictionResults",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "PredictionResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "PredictionResults",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "PredictionResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PrioritizedAtUtc",
                table: "PredictionResults",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrioritizedByUserId",
                table: "PredictionResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "PredictionResults",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "PredictionResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "PredictionJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "PredictionJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "PredictionJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StartedByUserId",
                table: "PredictionJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "PredictionJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "PredictionCandidates",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "PredictionCandidates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "PredictionCandidates",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "PredictionCandidates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAtUtc",
                table: "ModelDefinitions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "ModelDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ModelDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ModelDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "Compounds",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Compounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCreatedByUserId",
                table: "Compounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceUpdatedByUserId",
                table: "Compounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Compounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_CreatedByUserId",
                table: "Screenings",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_UpdatedByUserId",
                table: "Screenings",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningResults_CreatedByUserId",
                table: "ScreeningResults",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningResults_UpdatedByUserId",
                table: "ScreeningResults",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionResults_CreatedByUserId",
                table: "PredictionResults",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionResults_UpdatedByUserId",
                table: "PredictionResults",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionJobs_CreatedByUserId",
                table: "PredictionJobs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionJobs_UpdatedByUserId",
                table: "PredictionJobs",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionCandidates_CreatedByUserId",
                table: "PredictionCandidates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionCandidates_UpdatedByUserId",
                table: "PredictionCandidates",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelDefinitions_CreatedByUserId",
                table: "ModelDefinitions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelDefinitions_UpdatedByUserId",
                table: "ModelDefinitions",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Compounds_CreatedByUserId",
                table: "Compounds",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Compounds_UpdatedByUserId",
                table: "Compounds",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Screenings_CreatedByUserId",
                table: "Screenings");

            migrationBuilder.DropIndex(
                name: "IX_Screenings_UpdatedByUserId",
                table: "Screenings");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningResults_CreatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningResults_UpdatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropIndex(
                name: "IX_PredictionResults_CreatedByUserId",
                table: "PredictionResults");

            migrationBuilder.DropIndex(
                name: "IX_PredictionResults_UpdatedByUserId",
                table: "PredictionResults");

            migrationBuilder.DropIndex(
                name: "IX_PredictionJobs_CreatedByUserId",
                table: "PredictionJobs");

            migrationBuilder.DropIndex(
                name: "IX_PredictionJobs_UpdatedByUserId",
                table: "PredictionJobs");

            migrationBuilder.DropIndex(
                name: "IX_PredictionCandidates_CreatedByUserId",
                table: "PredictionCandidates");

            migrationBuilder.DropIndex(
                name: "IX_PredictionCandidates_UpdatedByUserId",
                table: "PredictionCandidates");

            migrationBuilder.DropIndex(
                name: "IX_ModelDefinitions_CreatedByUserId",
                table: "ModelDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ModelDefinitions_UpdatedByUserId",
                table: "ModelDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Compounds_CreatedByUserId",
                table: "Compounds");

            migrationBuilder.DropIndex(
                name: "IX_Compounds_UpdatedByUserId",
                table: "Compounds");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "SourceCreatedByUserId",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "SourceUpdatedByUserId",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "SourceCreatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "SourceUpdatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "PredictionResults");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "PredictionResults");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "PredictionResults");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PredictionResults");

            migrationBuilder.DropColumn(
                name: "PrioritizedAtUtc",
                table: "PredictionResults");

            migrationBuilder.DropColumn(
                name: "PrioritizedByUserId",
                table: "PredictionResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "PredictionResults");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PredictionResults");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "PredictionJobs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PredictionJobs");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "PredictionJobs");

            migrationBuilder.DropColumn(
                name: "StartedByUserId",
                table: "PredictionJobs");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PredictionJobs");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "PredictionCandidates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PredictionCandidates");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "PredictionCandidates");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PredictionCandidates");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "ModelDefinitions");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "ModelDefinitions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ModelDefinitions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ModelDefinitions");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Compounds");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Compounds");

            migrationBuilder.DropColumn(
                name: "SourceCreatedByUserId",
                table: "Compounds");

            migrationBuilder.DropColumn(
                name: "SourceUpdatedByUserId",
                table: "Compounds");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Compounds");
        }
    }
}
