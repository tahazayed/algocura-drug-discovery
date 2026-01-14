namespace Algocura.Workflow.Api.Data;

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

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<WorkflowDbContext>();

                // Replace with MigrateAsync after generating migrations.
                await dbContext.Database.EnsureCreatedAsync(
                    cancellationToken);

                logger.LogInformation(
                    "Workflow database initialization completed.");

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
                    "Workflow database initialization attempt " +
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
