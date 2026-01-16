using Microsoft.EntityFrameworkCore;

namespace Algocura.AIDiscovery.Api.Data;

public static class DatabaseInitializer
{
    public static readonly Guid PrototypeModelId =
        AIDiscoverySeeder.PrototypeModelId;

    public static async Task InitializeAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        const int maximumAttempts = 10;

        for (var attempt = 1;
             attempt <= maximumAttempts;
             attempt++)
        {
            try
            {
                await using var scope =
                    services.CreateAsyncScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<
                            AIDiscoveryDbContext>();

                logger.LogInformation(
                    "Applying AI Discovery database migrations.");

                await dbContext.Database.MigrateAsync(
                    cancellationToken);

                logger.LogInformation(
                    "Running AI Discovery seed process.");

                await AIDiscoverySeeder.SeedAsync(
                    dbContext,
                    cancellationToken);

                logger.LogInformation(
                    "AI Discovery database initialization completed.");

                return;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "AI Discovery database initialization was cancelled.");

                throw;
            }
            catch (Exception exception)
                when (attempt < maximumAttempts)
            {
                var delay =
                    TimeSpan.FromSeconds(
                        attempt * 2);

                logger.LogWarning(
                    exception,
                    "AI Discovery database initialization attempt " +
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
                    "AI Discovery database initialization failed " +
                    "after {MaximumAttempts} attempts.",
                    maximumAttempts);

                throw;
            }
        }
    }
}
