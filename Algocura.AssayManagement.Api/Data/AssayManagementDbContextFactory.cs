using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Algocura.AssayManagement.Api.Data;

public sealed class AssayManagementDbContextFactory
    : IDesignTimeDbContextFactory<AssayManagementDbContext>
{
    public AssayManagementDbContext CreateDbContext(
        string[] args)
    {
        var options =
            new DbContextOptionsBuilder<AssayManagementDbContext>()
                .UseSqlServer(
                    "Server=localhost,1433;" +
                    "Database=AlgocuraAssayManagement;" +
                    "User Id=sa;" +
                    "Password=DontLogin!Passw0rd;" +
                    "TrustServerCertificate=True")
                .Options;

        return new AssayManagementDbContext(options);
    }
}

