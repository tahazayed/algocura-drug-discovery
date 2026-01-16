using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Algocura.Search.Api.Data;

public sealed class SearchDbContextFactory
    : IDesignTimeDbContextFactory<SearchDbContext>
{
    public SearchDbContext CreateDbContext(
        string[] args)
    {
        var options =
            new DbContextOptionsBuilder<SearchDbContext>()
                .UseSqlServer(
                    "Server=localhost,1433;" +
                    "Database=AlgocuraSearch;" +
                    "User Id=sa;" +
                    "Password=DontLogin!Passw0rd;" +
                    "TrustServerCertificate=True")
                .Options;

        return new SearchDbContext(options);
    }
}

