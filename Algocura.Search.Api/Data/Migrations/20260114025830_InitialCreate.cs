using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.Search.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceEventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceService = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExternalReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WorkflowId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiseaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GeneSymbol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompoundName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InchiKey = table.Column<string>(type: "nvarchar(27)", maxLength: 27, nullable: true),
                    CanonicalSmiles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Score = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceUpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IndexedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchDocumentTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SearchDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDocumentTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchDocumentTags_SearchDocuments_SearchDocumentId",
                        column: x => x.SearchDocumentId,
                        principalTable: "SearchDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_CompoundName",
                table: "SearchDocuments",
                column: "CompoundName");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_DiseaseName",
                table: "SearchDocuments",
                column: "DiseaseName");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_DocumentType_SourceEntityId",
                table: "SearchDocuments",
                columns: new[] { "DocumentType", "SourceEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_GeneSymbol",
                table: "SearchDocuments",
                column: "GeneSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_InchiKey",
                table: "SearchDocuments",
                column: "InchiKey");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_Status",
                table: "SearchDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_Title",
                table: "SearchDocuments",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_WorkflowId",
                table: "SearchDocuments",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentTags_SearchDocumentId_Name_Value",
                table: "SearchDocumentTags",
                columns: new[] { "SearchDocumentId", "Name", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchDocumentTags");

            migrationBuilder.DropTable(
                name: "SearchDocuments");
        }
    }
}
