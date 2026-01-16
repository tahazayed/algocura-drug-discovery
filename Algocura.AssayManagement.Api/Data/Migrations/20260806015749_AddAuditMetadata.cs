using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.AssayManagement.Api.Data.Migrations
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

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "Screenings",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

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
                table: "Assays",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "Assays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "Assays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Assays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "Assays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PublishedByUserId",
                table: "Assays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Assays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAtUtc",
                table: "AssayResults",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "AssayResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "AssayResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SelectedForAiAtUtc",
                table: "AssayResults",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedForAiByUserId",
                table: "AssayResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "AssayResults",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "AssayResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CorrectedAtUtc",
                table: "AssayMeasurements",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrectedByUserId",
                table: "AssayMeasurements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "AssayMeasurements",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "AssayMeasurements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordedByUserId",
                table: "AssayMeasurements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "AssayMeasurements",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "AssayMeasurements",
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
                name: "IX_Assays_CreatedByUserId",
                table: "Assays",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assays_UpdatedByUserId",
                table: "Assays",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssayResults_CreatedByUserId",
                table: "AssayResults",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssayResults_UpdatedByUserId",
                table: "AssayResults",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssayMeasurements_CreatedByUserId",
                table: "AssayMeasurements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssayMeasurements_UpdatedByUserId",
                table: "AssayMeasurements",
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
                name: "IX_Assays_CreatedByUserId",
                table: "Assays");

            migrationBuilder.DropIndex(
                name: "IX_Assays_UpdatedByUserId",
                table: "Assays");

            migrationBuilder.DropIndex(
                name: "IX_AssayResults_CreatedByUserId",
                table: "AssayResults");

            migrationBuilder.DropIndex(
                name: "IX_AssayResults_UpdatedByUserId",
                table: "AssayResults");

            migrationBuilder.DropIndex(
                name: "IX_AssayMeasurements_CreatedByUserId",
                table: "AssayMeasurements");

            migrationBuilder.DropIndex(
                name: "IX_AssayMeasurements_UpdatedByUserId",
                table: "AssayMeasurements");

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
                name: "UpdatedAtUtc",
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
                table: "Assays");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Assays");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "Assays");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Assays");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Assays");

            migrationBuilder.DropColumn(
                name: "PublishedByUserId",
                table: "Assays");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Assays");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "AssayResults");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "AssayResults");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AssayResults");

            migrationBuilder.DropColumn(
                name: "SelectedForAiAtUtc",
                table: "AssayResults");

            migrationBuilder.DropColumn(
                name: "SelectedForAiByUserId",
                table: "AssayResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "AssayResults");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "AssayResults");

            migrationBuilder.DropColumn(
                name: "CorrectedAtUtc",
                table: "AssayMeasurements");

            migrationBuilder.DropColumn(
                name: "CorrectedByUserId",
                table: "AssayMeasurements");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "AssayMeasurements");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AssayMeasurements");

            migrationBuilder.DropColumn(
                name: "RecordedByUserId",
                table: "AssayMeasurements");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "AssayMeasurements");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "AssayMeasurements");
        }
    }
}
