using Algocura.Platform.Messaging.Publishing;
using Algocura.Search.Api.Data;
using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Search.Api.Controllers;

[ApiController]
[Route("api/v1/search/synchronization")]
public sealed class SearchSynchronizationController(
    SearchDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<SearchSynchronizationController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SearchSynchronizationResponse>>
        PublishIndexSnapshot(
            CancellationToken cancellationToken = default)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;

        var activeDocuments =
            await dbContext.SearchDocuments
                .AsNoTracking()
                .Where(
                    x => x.Status == SearchDocumentStatus.Active)
                .ToListAsync(cancellationToken);

        var counts =
            activeDocuments
                .GroupBy(x => x.DocumentType)
                .ToDictionary(
                    group => group.Key.ToString(),
                    group => group.Count());

        try
        {
            await eventPublisher.PublishAsync(
                new SearchIndexSnapshot(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    activeDocuments.Count,
                    counts),
                "search-index-snapshot",
                cancellationToken);

            return Ok(
                new SearchSynchronizationResponse(
                    activeDocuments.Count,
                    counts,
                    startedAtUtc,
                    DateTimeOffset.UtcNow,
                    true,
                    null));
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to publish search index snapshot.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new SearchSynchronizationResponse(
                    activeDocuments.Count,
                    counts,
                    startedAtUtc,
                    DateTimeOffset.UtcNow,
                    false,
                    exception.Message));
        }
    }
}

public sealed record SearchSynchronizationResponse(
    int ActiveDocumentCount,
    IReadOnlyDictionary<string, int> DocumentTypeCounts,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    bool Published,
    string? Error);

