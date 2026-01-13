using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Algocura.TargetDiscovery.Api.Data;

public sealed class TargetDiscoveryDbContextFactory
    : IDesignTimeDbContextFactory<TargetDiscoveryDbContext>
{
    public TargetDiscoveryDbContext CreateDbContext(string[] args)
    {
        var options =
            new DbContextOptionsBuilder<TargetDiscoveryDbContext>()
                .UseSqlServer(
                    "Server=localhost,1433;" +
                    "Database=AlgocuraTargetDiscovery;" +
                    "User Id=sa;" +
                    "Password=DontLogin!Passw0rd;" +
                    "TrustServerCertificate=True")
                .Options;

        return new TargetDiscoveryDbContext(options);
    }
}
