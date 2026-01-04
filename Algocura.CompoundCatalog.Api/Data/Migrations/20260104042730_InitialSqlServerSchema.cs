using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.CompoundCatalog.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServerSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Compounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CanonicalSmiles = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    InchiKey = table.Column<string>(type: "nvarchar(27)", maxLength: 27, nullable: true),
                    MolecularWeight = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    MolecularFormula = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compounds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compounds_InchiKey",
                table: "Compounds",
                column: "InchiKey",
                unique: true,
                filter: "[InchiKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Compounds_Name",
                table: "Compounds",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Compounds_Status",
                table: "Compounds",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compounds");
        }
    }
}
