using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Algocura.Notification.Api.Data;

public sealed class NotificationDbContextFactory
    : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseSqlServer(
                "Server=localhost,1433;Database=AlgocuraNotification;" +
                "User Id=sa;Password=DontLogin!Passw0rd;TrustServerCertificate=True")
            .Options;

        return new NotificationDbContext(options);
    }
}
