using Microsoft.EntityFrameworkCore;

namespace Algocura.Screening.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ScreeningDbContext>();
                await db.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Screening database initialization completed.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(30, attempt * 3));
                logger.LogWarning(ex, "Database initialization attempt {Attempt}/{MaxAttempts} failed. Retrying in {Delay}.", attempt, maxAttempts, delay);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}