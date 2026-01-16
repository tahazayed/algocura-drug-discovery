using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.CompoundCatalog.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Compounds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Compounds",
                type: "uniqueidentifier",
                nullable: true);

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
                name: "IX_Compounds_CreatedByUserId",
                table: "Compounds");

            migrationBuilder.DropIndex(
                name: "IX_Compounds_UpdatedByUserId",
                table: "Compounds");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Compounds");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Compounds");
        }
    }
}
