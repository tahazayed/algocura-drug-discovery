using Microsoft.EntityFrameworkCore;

namespace Algocura.TargetDiscovery.Api.Data;

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

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<TargetDiscoveryDbContext>();

                logger.LogInformation(
                    "Creating or validating the Target Discovery database.");

                // Replace with MigrateAsync after generating the initial migration.
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);

                logger.LogInformation(
                    "Target Discovery database initialization completed.");

                return;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "Target Discovery database initialization was cancelled.");

                throw;
            }
            catch (Exception exception)
                when (attempt < maximumAttempts)
            {
                var delay = TimeSpan.FromSeconds(attempt * 2);

                logger.LogWarning(
                    exception,
                    "Target Discovery database initialization attempt " +
                    "{Attempt} of {MaximumAttempts} failed. " +
                    "Retrying in {DelaySeconds} seconds.",
                    attempt,
                    maximumAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogCritical(
                    exception,
                    "Target Discovery database initialization failed " +
                    "after {MaximumAttempts} attempts.",
                    maximumAttempts);

                throw;
            }
        }
    }
}
