using Algocura.ClinicalResearch.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ClinicalResearch.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection connection;
    private TestDatabase(SqliteConnection connection, ClinicalResearchDbContext context) { this.connection = connection; Context = context; }
    public ClinicalResearchDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ClinicalResearchDbContext>().UseSqlite(connection).Options;
        var context = new ClinicalResearchDbContext(options);
        context.Database.EnsureCreated();
        return new TestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync() { await Context.DisposeAsync(); await connection.DisposeAsync(); }
}
