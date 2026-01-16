using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.TargetDiscovery.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "TargetHypothesisEvidence",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "TargetHypothesisEvidence",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "TargetHypothesisEvidence",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "TargetHypothesisEvidence",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "TargetHypotheses",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "TargetHypotheses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "TargetHypotheses",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "TargetHypotheses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "TargetAssessments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "TargetAssessments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TargetHypothesisEvidence_CreatedByUserId",
                table: "TargetHypothesisEvidence",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetHypothesisEvidence_UpdatedByUserId",
                table: "TargetHypothesisEvidence",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetHypotheses_CreatedByUserId",
                table: "TargetHypotheses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetHypotheses_UpdatedByUserId",
                table: "TargetHypotheses",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TargetHypothesisEvidence_CreatedByUserId",
                table: "TargetHypothesisEvidence");

            migrationBuilder.DropIndex(
                name: "IX_TargetHypothesisEvidence_UpdatedByUserId",
                table: "TargetHypothesisEvidence");

            migrationBuilder.DropIndex(
                name: "IX_TargetHypotheses_CreatedByUserId",
                table: "TargetHypotheses");

            migrationBuilder.DropIndex(
                name: "IX_TargetHypotheses_UpdatedByUserId",
                table: "TargetHypotheses");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "TargetHypothesisEvidence");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TargetHypothesisEvidence");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "TargetHypothesisEvidence");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "TargetHypothesisEvidence");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "TargetHypotheses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TargetHypotheses");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "TargetHypotheses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "TargetHypotheses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TargetAssessments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "TargetAssessments");
        }
    }
}
