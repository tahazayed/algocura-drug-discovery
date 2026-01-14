using Algocura.Workflow.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Workflow.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(
        SqliteConnection connection,
        WorkflowDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public WorkflowDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        connection.Open();

        var options =
            new DbContextOptionsBuilder<WorkflowDbContext>()
                .UseSqlite(connection)
                .Options;

        var context =
            new WorkflowDbContext(options);

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
