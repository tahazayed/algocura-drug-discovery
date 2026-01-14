using Microsoft.EntityFrameworkCore;

namespace Algocura.AssayManagement.Api.Data;

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
                        .GetRequiredService<AssayManagementDbContext>();

                logger.LogInformation(
                    "Creating or validating the Assay Management database.");

                // Replace with MigrateAsync after generating the initial migration.
                await dbContext.Database.MigrateAsync(
                    cancellationToken);

                logger.LogInformation(
                    "Assay Management database initialization completed.");

                return;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
                when (attempt < maximumAttempts)
            {
                var delay =
                    TimeSpan.FromSeconds(attempt * 2);

                logger.LogWarning(
                    exception,
                    "Assay Management initialization attempt " +
                    "{Attempt} of {MaximumAttempts} failed. " +
                    "Retrying in {DelaySeconds} seconds.",
                    attempt,
                    maximumAttempts,
                    delay.TotalSeconds);

                await Task.Delay(
                    delay,
                    cancellationToken);
            }
        }
    }
}
