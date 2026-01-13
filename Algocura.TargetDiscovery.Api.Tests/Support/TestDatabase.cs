using Algocura.TargetDiscovery.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.TargetDiscovery.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(
        SqliteConnection connection,
        TargetDiscoveryDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public TargetDiscoveryDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection =
            new SqliteConnection("Data Source=:memory:");

        connection.Open();

        var options =
            new DbContextOptionsBuilder<TargetDiscoveryDbContext>()
                .UseSqlite(connection)
                .Options;

        var context =
            new TargetDiscoveryDbContext(options);

        context.Database.EnsureCreated();

        return new TestDatabase(
            connection,
            context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
