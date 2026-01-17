using Microsoft.EntityFrameworkCore;

namespace Algocura.Audit.Api.Data;

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
                await using var scope =
                    services.CreateAsyncScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<AuditDbContext>();

                logger.LogInformation(
                    "Applying Audit database migrations.");

                await dbContext.Database.MigrateAsync(
                    cancellationToken);

                logger.LogInformation(
                    "Running Audit seed process.");

                await AuditSeeder.SeedAsync(
                    dbContext,
                    cancellationToken);

                logger.LogInformation(
                    "Audit database initialization completed.");

                return;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "Audit database initialization was cancelled.");

                throw;
            }
            catch (Exception exception)
                when (attempt < maximumAttempts)
            {
                var delay =
                    TimeSpan.FromSeconds(attempt * 2);

                logger.LogWarning(
                    exception,
                    "Audit database initialization attempt " +
                    "{Attempt} of {MaximumAttempts} failed. " +
                    "Retrying in {DelaySeconds} seconds.",
                    attempt,
                    maximumAttempts,
                    delay.TotalSeconds);

                await Task.Delay(
                    delay,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogCritical(
                    exception,
                    "Audit database initialization failed " +
                    "after {MaximumAttempts} attempts.",
                    maximumAttempts);

                throw;
            }
        }
    }
}
