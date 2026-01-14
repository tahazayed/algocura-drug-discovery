using Algocura.AssayManagement.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AssayManagement.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(
        SqliteConnection connection,
        AssayManagementDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public AssayManagementDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection =
            new SqliteConnection("Data Source=:memory:");

        connection.Open();

        var options =
            new DbContextOptionsBuilder<AssayManagementDbContext>()
                .UseSqlite(connection)
                .Options;

        var context =
            new AssayManagementDbContext(options);

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
