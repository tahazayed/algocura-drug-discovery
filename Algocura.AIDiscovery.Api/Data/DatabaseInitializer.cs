using Algocura.AIDiscovery.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AIDiscovery.Api.Data;

public static class DatabaseInitializer
{
    public static readonly Guid PrototypeModelId =
        Guid.Parse("48d78c3a-0f59-4d43-baca-830c976988d6");

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
                        .GetRequiredService<AIDiscoveryDbContext>();

                logger.LogInformation(
                    "Applying AI Discovery database migrations.");

                await dbContext.Database.MigrateAsync(
                    cancellationToken);

                logger.LogInformation(
                    "Running AI Discovery seed process.");

                if (!await dbContext.ModelDefinitions.AnyAsync(
                        model =>
                            model.Id == PrototypeModelId,
                        cancellationToken))
                {
                    var timestamp =
                        new DateTimeOffset(
                            2026,
                            1,
                            1,
                            0,
                            0,
                            0,
                            TimeSpan.Zero);

                    dbContext.ModelDefinitions.Add(
                        new ModelDefinition
                        {
                            Id = PrototypeModelId,

                            Name =
                                "Algocura Deterministic Prioritizer",

                            Version = "1.0",

                            Description =
                                "Prototype deterministic scoring model.",

                            ScreeningScoreWeight = 0.75m,

                            MolecularWeightWeight = 0.15m,

                            SelectionBonus = 10m,

                            Status = ModelStatus.Active,

                            CreatedAtUtc = timestamp,

                            UpdatedAtUtc = timestamp
                        });

                    await dbContext.SaveChangesAsync(
                        cancellationToken);
                }

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
                    TimeSpan.FromSeconds(attempt * 2);

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