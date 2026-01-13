using Algocura.ChemicalDesign.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ChemicalDesign.Api.Data;

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
                var db = scope.ServiceProvider.GetRequiredService<ChemicalDesignDbContext>();

                await db.Database.MigrateAsync(cancellationToken);

                if (!await db.ChemicalLibrary.AnyAsync(cancellationToken))
                {
                    db.ChemicalLibrary.AddRange(
                        Entry("Aspirin", "CC(=O)OC1=CC=CC=C1C(=O)O", "BSYNRYMUTXBXSQ-UHFFFAOYSA-N", "C9H8O4", 180.16m),
                        Entry("Caffeine", "CN1C=NC2=C1C(=O)N(C(=O)N2C)C", "RYYVLZVUVIJVGH-UHFFFAOYSA-N", "C8H10N4O2", 194.19m),
                        Entry("Acetaminophen", "CC(=O)NC1=CC=C(C=C1)O", "RZVAJINKPMORJF-UHFFFAOYSA-N", "C8H9NO2", 151.16m),
                        Entry("Ibuprofen", "CC(C)CC1=CC=C(C=C1)C(C)C(=O)O", "HEFNNWSXXWATRW-UHFFFAOYSA-N", "C13H18O2", 206.28m),
                        Entry("Naproxen", "COC1=CC2=C(C=C1)C=C(C=C2)[C@@H](C)C(=O)O", "CMWTZPSULFXXJA-VIFPVBQESA-N", "C14H14O3", 230.26m));

                    await db.SaveChangesAsync(cancellationToken);
                }

                logger.LogInformation("Chemical Design database initialization completed.");
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
                    "Chemical Design initialization attempt {Attempt} of {MaximumAttempts} failed. Retrying in {DelaySeconds} seconds.",
                    attempt, maximumAttempts, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static ChemicalLibraryEntry Entry(
        string name, string smiles, string inchiKey, string formula, decimal weight) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            CanonicalSmiles = smiles,
            InchiKey = inchiKey,
            MolecularFormula = formula,
            MolecularWeight = weight,
            SourceReference = "Prototype chemical library"
        };
}
