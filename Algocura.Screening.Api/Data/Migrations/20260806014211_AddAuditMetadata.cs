using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.Screening.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ScreeningResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SelectedAtUtc",
                table: "ScreeningResults",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedByUserId",
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

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "ScreeningJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ScreeningJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "ScreeningJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StartedByUserId",
                table: "ScreeningJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ScreeningJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "ScreeningCompounds",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ScreeningCompounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCreatedByUserId",
                table: "ScreeningCompounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceUpdatedByUserId",
                table: "ScreeningCompounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ScreeningCompounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "ScreeningCandidates",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ScreeningCandidates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "ScreeningCandidates",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ScreeningCandidates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningResults_CreatedByUserId",
                table: "ScreeningResults",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningResults_UpdatedByUserId",
                table: "ScreeningResults",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_CreatedByUserId",
                table: "ScreeningJobs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_UpdatedByUserId",
                table: "ScreeningJobs",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningCompounds_CreatedByUserId",
                table: "ScreeningCompounds",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningCompounds_UpdatedByUserId",
                table: "ScreeningCompounds",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningCandidates_CreatedByUserId",
                table: "ScreeningCandidates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningCandidates_UpdatedByUserId",
                table: "ScreeningCandidates",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScreeningResults_CreatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningResults_UpdatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningJobs_CreatedByUserId",
                table: "ScreeningJobs");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningJobs_UpdatedByUserId",
                table: "ScreeningJobs");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningCompounds_CreatedByUserId",
                table: "ScreeningCompounds");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningCompounds_UpdatedByUserId",
                table: "ScreeningCompounds");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningCandidates_CreatedByUserId",
                table: "ScreeningCandidates");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningCandidates_UpdatedByUserId",
                table: "ScreeningCandidates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "SelectedAtUtc",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "SelectedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ScreeningResults");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "ScreeningJobs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ScreeningJobs");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "ScreeningJobs");

            migrationBuilder.DropColumn(
                name: "StartedByUserId",
                table: "ScreeningJobs");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ScreeningJobs");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ScreeningCompounds");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ScreeningCompounds");

            migrationBuilder.DropColumn(
                name: "SourceCreatedByUserId",
                table: "ScreeningCompounds");

            migrationBuilder.DropColumn(
                name: "SourceUpdatedByUserId",
                table: "ScreeningCompounds");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ScreeningCompounds");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ScreeningCandidates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ScreeningCandidates");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ScreeningCandidates");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ScreeningCandidates");
        }
    }
}
