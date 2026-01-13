using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Algocura.ChemicalDesign.Api.Data;

public sealed class ChemicalDesignDbContextFactory
    : IDesignTimeDbContextFactory<ChemicalDesignDbContext>
{
    public ChemicalDesignDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ChemicalDesignDbContext>()
            .UseSqlServer(
                "Server=localhost,1433;Database=AlgocuraChemicalDesign;" +
                "User Id=sa;Password=DontLogin!Passw0rd;TrustServerCertificate=True")
            .Options;

        return new ChemicalDesignDbContext(options);
    }
}
