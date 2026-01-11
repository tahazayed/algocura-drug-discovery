using Algocura.Screening.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Screening.Api.Tests.Support;

internal static class TestDb
{
    public static ScreeningDbContext Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ScreeningDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ScreeningDbContext(options);

        db.Database.EnsureCreated();

        return db;
    }
}
