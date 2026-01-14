using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Algocura.Workflow.Api.Data;

public sealed class WorkflowDbContextFactory
    : IDesignTimeDbContextFactory<WorkflowDbContext>
{
    public WorkflowDbContext CreateDbContext(string[] args)
    {
        var options =
            new DbContextOptionsBuilder<WorkflowDbContext>()
                .UseSqlServer(
                    "Server=localhost,1433;" +
                    "Database=AlgocuraWorkflow;" +
                    "User Id=sa;" +
                    "Password=DontLogin!Passw0rd;" +
                    "TrustServerCertificate=True")
                .Options;

        return new WorkflowDbContext(options);
    }
}
