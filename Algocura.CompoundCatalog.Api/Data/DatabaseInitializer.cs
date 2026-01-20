using Algocura.CompoundCatalog.Api.Events;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.EntityFrameworkCore;

namespace Algocura.CompoundCatalog.Api.Data;

public static class DatabaseInitializer
{
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

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<
                        CompoundCatalogDbContext>();

                var eventPublisher = scope.ServiceProvider
                    .GetRequiredService<IEventPublisher>();

                logger.LogInformation(
                    "Applying Compound Catalog database migrations.");

                await dbContext.Database.MigrateAsync(
                    cancellationToken);

                logger.LogInformation(
                    "Running Compound Catalog seed process.");

                var seedCompounds =
                    await CompoundCatalogSeeder.SeedAsync(
                        dbContext,
                        cancellationToken);

                logger.LogInformation(
                    "Publishing {CompoundCount} seed compounds " +
                    "to Azure Service Bus.",
                    seedCompounds.Count);

                foreach (var compound in seedCompounds)
                {
                    var integrationEvent =
                        new CompoundRegistered
                        {
                            CompoundId = compound.Id,
                            Name = compound.Name,
                            CanonicalSmiles =
                                compound.CanonicalSmiles,
                            InchiKey = compound.InchiKey,
                            MolecularWeight =
                                compound.MolecularWeight,
                            MolecularFormula =
                                compound.MolecularFormula,
                            Status = compound.Status
                        };

                    await eventPublisher.PublishAsync(
                        integrationEvent,
                        correlationId:
                            $"compound-seed-{compound.Id}",
                        cancellationToken:
                            cancellationToken);

                    logger.LogInformation(
                        "Published seeded compound {CompoundId} " +
                        "with name {CompoundName}.",
                        compound.Id,
                        compound.Name);
                }

                logger.LogInformation(
                    "Compound Catalog database initialization " +
                    "completed. Published {CompoundCount} " +
                    "seed compound events.",
                    seedCompounds.Count);

                return;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "Compound Catalog database initialization " +
                    "was cancelled.");

                throw;
            }
            catch (Exception exception)
                when (attempt < maximumAttempts)
            {
                var delay =
                    TimeSpan.FromSeconds(attempt * 2);

                logger.LogWarning(
                    exception,
                    "Compound Catalog database initialization " +
                    "attempt {Attempt} of {MaximumAttempts} failed. " +
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
                    "Compound Catalog database initialization failed " +
                    "after {MaximumAttempts} attempts.",
                    maximumAttempts);

                throw;
            }
        }
    }
}
