using Algocura.Audit.Api.Contracts;
using Algocura.Audit.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Audit.Api.Controllers;

[ApiController]
[Route("api/v1/audit")]
public sealed class AuditController(
    AuditDbContext dbContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AuditSearchResponse>> Search(
        [FromQuery] string? serviceName,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? action,
        [FromQuery] Guid? userId,
        [FromQuery] string? correlationId,
        [FromQuery] string? workflowId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query =
            dbContext.AuditRecords
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            query = query.Where(
                x => x.ServiceName == serviceName);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(
                x => x.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            query = query.Where(
                x => x.EntityId == entityId);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(
                x => x.Action == action);
        }

        if (userId.HasValue)
        {
            query = query.Where(
                x => x.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            query = query.Where(
                x => x.CorrelationId == correlationId);
        }

        if (!string.IsNullOrWhiteSpace(workflowId))
        {
            query = query.Where(
                x => x.WorkflowId == workflowId);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(
                x => x.OccurredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(
                x => x.OccurredAtUtc <= toUtc.Value);
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var items =
            await query
                .OrderByDescending(
                    x => x.OccurredAtUtc)
                .ThenByDescending(
                    x => x.EventId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x =>
                    new AuditRecordResponse(
                        x.EventId,
                        x.OccurredAtUtc,
                        x.ServiceName,
                        x.EntityType,
                        x.EntityId,
                        x.Action,
                        x.UserId,
                        x.Subject,
                        x.UserDisplayName,
                        x.UserEmail,
                        x.ActorType,
                        x.PreviousValuesJson,
                        x.CurrentValuesJson,
                        x.ChangedPropertiesJson,
                        x.CorrelationId,
                        x.WorkflowId,
                        x.TraceId,
                        x.IpAddress,
                        x.UserAgent,
                        x.EventType,
                        x.EventVersion,
                        x.ReceivedAtUtc))
                .ToListAsync(cancellationToken);

        return Ok(
            new AuditSearchResponse(
                totalCount,
                page,
                pageSize,
                items));
    }

    [HttpGet("{eventId:guid}")]
    public async Task<ActionResult<AuditRecordResponse>> GetById(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var item =
            await dbContext.AuditRecords
                .AsNoTracking()
                .Where(x => x.EventId == eventId)
                .Select(x =>
                    new AuditRecordResponse(
                        x.EventId,
                        x.OccurredAtUtc,
                        x.ServiceName,
                        x.EntityType,
                        x.EntityId,
                        x.Action,
                        x.UserId,
                        x.Subject,
                        x.UserDisplayName,
                        x.UserEmail,
                        x.ActorType,
                        x.PreviousValuesJson,
                        x.CurrentValuesJson,
                        x.ChangedPropertiesJson,
                        x.CorrelationId,
                        x.WorkflowId,
                        x.TraceId,
                        x.IpAddress,
                        x.UserAgent,
                        x.EventType,
                        x.EventVersion,
                        x.ReceivedAtUtc))
                .SingleOrDefaultAsync(
                    cancellationToken);

        return item is null
            ? NotFound()
            : Ok(item);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AuditSummaryResponse>> Summary(
        CancellationToken cancellationToken = default)
    {
        var total =
            await dbContext.AuditRecords
                .CountAsync(cancellationToken);

        var byService =
            await dbContext.AuditRecords
                .AsNoTracking()
                .GroupBy(x => x.ServiceName)
                .Select(x => new
                {
                    Key = x.Key,
                    Count = x.Count()
                })
                .ToDictionaryAsync(
                    x => x.Key,
                    x => x.Count,
                    cancellationToken);

        var byAction =
            await dbContext.AuditRecords
                .AsNoTracking()
                .GroupBy(x => x.Action)
                .Select(x => new
                {
                    Key = x.Key,
                    Count = x.Count()
                })
                .ToDictionaryAsync(
                    x => x.Key,
                    x => x.Count,
                    cancellationToken);

        var byActorType =
            await dbContext.AuditRecords
                .AsNoTracking()
                .GroupBy(x => x.ActorType)
                .Select(x => new
                {
                    Key = x.Key,
                    Count = x.Count()
                })
                .ToDictionaryAsync(
                    x => x.Key,
                    x => x.Count,
                    cancellationToken);

        return Ok(
            new AuditSummaryResponse(
                total,
                byService,
                byAction,
                byActorType));
    }
}
