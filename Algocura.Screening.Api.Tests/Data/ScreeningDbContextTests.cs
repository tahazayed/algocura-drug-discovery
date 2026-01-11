using Algocura.Screening.Api.Domain;
using Algocura.Screening.Api.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Screening.Api.Tests.Data;

public sealed class ScreeningDbContextTests
{
    [Fact]
    public async Task SaveAndLoad_ScreeningWithCandidatesAndResults_Works()
    {
        await using var db = TestDb.Create();
        var compoundId = Guid.NewGuid();
        db.ScreeningJobs.Add(new ScreeningJob
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            ScreeningMethod = "Docking",
            Status = ScreeningStatus.Completed,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Candidates = [new ScreeningCandidate { Id = Guid.NewGuid(), CompoundId = compoundId, CompoundName = "A", CanonicalSmiles = "C" }],
            Results = [new ScreeningResult { Id = Guid.NewGuid(), CompoundId = compoundId, Score = 0.99m, Rank = 1, Selected = true, CreatedAtUtc = DateTimeOffset.UtcNow }]
        });
        await db.SaveChangesAsync();
        var loaded = await db.ScreeningJobs.Include(x => x.Candidates).Include(x => x.Results).SingleAsync();
        loaded.Candidates.Should().ContainSingle();
        loaded.Results.Should().ContainSingle(x => x.Selected);
    }
}
