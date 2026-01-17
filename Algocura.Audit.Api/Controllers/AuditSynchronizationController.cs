using Algocura.Audit.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Audit.Api.Controllers;

[ApiController]
[Route("api/v1/audit/synchronization")]
public sealed class AuditSynchronizationController(
    AuditDbContext dbContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AuditSynchronizationResponse>>
        GetSnapshot(
            CancellationToken cancellationToken = default)
    {
        var total =
            await dbContext.AuditRecords
                .CountAsync(cancellationToken);

        var latest =
            await dbContext.AuditRecords
                .AsNoTracking()
                .OrderByDescending(x => x.OccurredAtUtc)
                .Select(x => (DateTimeOffset?)x.OccurredAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

        return Ok(
            new AuditSynchronizationResponse(
                total,
                latest,
                DateTimeOffset.UtcNow));
    }
}

public sealed record AuditSynchronizationResponse(
    int TotalAuditEvents,
    DateTimeOffset? LatestOccurredAtUtc,
    DateTimeOffset GeneratedAtUtc);
