using Microsoft.EntityFrameworkCore;

namespace Algocura.Reporting.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        const int maximumAttempts = 10;

        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();

                logger.LogInformation("Applying Reporting database migrations.");
                await db.Database.MigrateAsync(cancellationToken);

                logger.LogInformation("Running Reporting seed process.");
                await ReportingSeeder.SeedAsync(db, cancellationToken);

                logger.LogInformation("Reporting database initialization completed.");
                return;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Reporting database initialization was cancelled.");
                throw;
            }
            catch (Exception exception) when (attempt < maximumAttempts)
            {
                var delay = TimeSpan.FromSeconds(attempt * 2);
                logger.LogWarning(
                    exception,
                    "Reporting initialization attempt {Attempt} of {MaximumAttempts} failed. Retrying in {DelaySeconds} seconds.",
                    attempt,
                    maximumAttempts,
                    delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogCritical(
                    exception,
                    "Reporting database initialization failed after {MaximumAttempts} attempts.",
                    maximumAttempts);
                throw;
            }
        }
    }
}
