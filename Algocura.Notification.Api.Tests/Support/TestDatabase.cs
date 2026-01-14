using Algocura.Notification.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Notification.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(
        SqliteConnection connection,
        NotificationDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public NotificationDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new NotificationDbContext(options);
        context.Database.EnsureCreated();

        return new TestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
