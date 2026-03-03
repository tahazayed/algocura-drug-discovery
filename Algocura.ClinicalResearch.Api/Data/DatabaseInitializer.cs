using Microsoft.EntityFrameworkCore;

namespace Algocura.ClinicalResearch.Api.Data;

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
                var db = scope.ServiceProvider.GetRequiredService<ClinicalResearchDbContext>();

                logger.LogInformation("Initializing Clinical Research database schema.");

                // The new prototype service uses EnsureCreated so it can run immediately
                // without requiring a generated migration in this delivery. Before a
                // production rollout, replace this with Database.MigrateAsync and add
                // a normal EF Core migration to the project.
                await db.Database.MigrateAsync(cancellationToken);

                logger.LogInformation("Clinical Research database initialization completed.");
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (attempt < maximumAttempts)
            {
                var delay = TimeSpan.FromSeconds(attempt * 2);
                logger.LogWarning(exception,
                    "Clinical Research database initialization attempt {Attempt} of {MaximumAttempts} failed. Retrying in {DelaySeconds} seconds.",
                    attempt, maximumAttempts, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
