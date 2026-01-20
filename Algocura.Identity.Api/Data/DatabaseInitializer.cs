using Microsoft.EntityFrameworkCore;

namespace Algocura.Identity.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
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
                            IdentityDbContext>();

                logger.LogInformation(
                    "Applying Identity database migrations.");

                await dbContext.Database.MigrateAsync(
                    cancellationToken);

                logger.LogInformation(
                    "Running Identity seed process.");

                await IdentitySeeder.SeedAsync(
                    scope.ServiceProvider,
                    configuration,
                    cancellationToken);

                logger.LogInformation(
                    "Identity database initialization completed.");

                return;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "Identity database initialization was cancelled.");

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
                    "Identity database initialization attempt " +
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
                    "Identity database initialization failed after " +
                    "{MaximumAttempts} attempts.",
                    maximumAttempts);

                throw;
            }
        }
    }
}
