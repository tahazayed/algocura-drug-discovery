using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.Search.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "SearchDocumentTags",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "SearchDocumentTags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "SearchDocumentTags",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "SearchDocumentTags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "SearchDocuments",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "SearchDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCreatedByUserId",
                table: "SearchDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceUpdatedByUserId",
                table: "SearchDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "SearchDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentTags_CreatedByUserId",
                table: "SearchDocumentTags",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentTags_UpdatedByUserId",
                table: "SearchDocumentTags",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_CreatedByUserId",
                table: "SearchDocuments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_UpdatedByUserId",
                table: "SearchDocuments",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SearchDocumentTags_CreatedByUserId",
                table: "SearchDocumentTags");

            migrationBuilder.DropIndex(
                name: "IX_SearchDocumentTags_UpdatedByUserId",
                table: "SearchDocumentTags");

            migrationBuilder.DropIndex(
                name: "IX_SearchDocuments_CreatedByUserId",
                table: "SearchDocuments");

            migrationBuilder.DropIndex(
                name: "IX_SearchDocuments_UpdatedByUserId",
                table: "SearchDocuments");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "SearchDocumentTags");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SearchDocumentTags");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "SearchDocumentTags");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "SearchDocumentTags");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "SearchDocuments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SearchDocuments");

            migrationBuilder.DropColumn(
                name: "SourceCreatedByUserId",
                table: "SearchDocuments");

            migrationBuilder.DropColumn(
                name: "SourceUpdatedByUserId",
                table: "SearchDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "SearchDocuments");
        }
    }
}
