using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algocura.Screening.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScreeningCompounds",
                columns: table => new
                {
                    CompoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CanonicalSmiles = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    InchiKey = table.Column<string>(type: "nvarchar(27)", maxLength: 27, nullable: true),
                    RegisteredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningCompounds", x => x.CompoundId);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ScreeningMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScreeningJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompoundName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CanonicalSmiles = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AddedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningCandidates_ScreeningJobs_ScreeningJobId",
                        column: x => x.ScreeningJobId,
                        principalTable: "ScreeningJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScreeningJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Selected = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningResults_ScreeningJobs_ScreeningJobId",
                        column: x => x.ScreeningJobId,
                        principalTable: "ScreeningJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningCandidates_ScreeningJobId_CompoundId",
                table: "ScreeningCandidates",
                columns: new[] { "ScreeningJobId", "CompoundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningCompounds_InchiKey",
                table: "ScreeningCompounds",
                column: "InchiKey");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_CreatedAtUtc",
                table: "ScreeningJobs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_Status",
                table: "ScreeningJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningResults_ScreeningJobId_CompoundId",
                table: "ScreeningResults",
                columns: new[] { "ScreeningJobId", "CompoundId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScreeningCandidates");

            migrationBuilder.DropTable(
                name: "ScreeningCompounds");

            migrationBuilder.DropTable(
                name: "ScreeningResults");

            migrationBuilder.DropTable(
                name: "ScreeningJobs");
        }
    }
}
