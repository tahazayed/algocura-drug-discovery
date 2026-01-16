using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.LiteratureIntelligence.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "TargetHypothesisEvidence",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "TargetHypothesisEvidence",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "TargetHypotheses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "TargetHypotheses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Publications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Publications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "PublicationAuthors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "PublicationAuthors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "EvidenceStatements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "EvidenceStatements",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TargetHypothesisEvidence");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "TargetHypothesisEvidence");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TargetHypotheses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "TargetHypotheses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PublicationAuthors");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PublicationAuthors");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "EvidenceStatements");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "EvidenceStatements");
        }
    }
}
