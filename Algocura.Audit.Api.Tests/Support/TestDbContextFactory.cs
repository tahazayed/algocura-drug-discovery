using Algocura.Audit.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Audit.Api.Tests.Support;

internal static class TestDbContextFactory
{
    public static Task<AuditDbContext> CreateAsync()
    {
        var options =
            new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase(
                    $"AuditTests-{Guid.NewGuid()}")
                .Options;

        var dbContext =
            new AuditDbContext(options);

        return Task.FromResult(dbContext);
    }
}