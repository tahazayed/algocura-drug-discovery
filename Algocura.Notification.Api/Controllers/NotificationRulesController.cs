using Algocura.Notification.Api.Contracts;
using Algocura.Notification.Api.Data;
using Algocura.Notification.Api.Domain;
using Algocura.Notification.Api.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Notification.Api.Controllers;

[ApiController]
[Route("api/v1/notification-rules")]
public sealed class NotificationRulesController(
    NotificationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationRuleResponse>>> GetAll(
        string? eventType,
        bool? enabled,
        CancellationToken cancellationToken = default)
    {
        var query = db.NotificationRules.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(x => x.EventType == eventType.Trim());

        if (enabled.HasValue)
            query = query.Where(x => x.Enabled == enabled.Value);

        var rules = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return Ok(rules.Select(x => x.ToResponse()).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationRuleResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await db.NotificationRules.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return rule is null ? NotFound() : Ok(rule.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<NotificationRuleResponse>> Create(
        CreateNotificationRuleRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var rule = new NotificationRule
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            EventType = request.EventType.Trim(),
            Channel = request.Channel,
            Recipient = request.Recipient.Trim(),
            SubjectTemplate = request.SubjectTemplate.Trim(),
            BodyTemplate = request.BodyTemplate.Trim(),
            Priority = request.Priority,
            Enabled = request.Enabled,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.NotificationRules.Add(rule);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = rule.Id },
            rule.ToResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NotificationRuleResponse>> Update(
        Guid id,
        UpdateNotificationRuleRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await db.NotificationRules
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (rule is null) return NotFound();

        rule.Name = request.Name.Trim();
        rule.EventType = request.EventType.Trim();
        rule.Channel = request.Channel;
        rule.Recipient = request.Recipient.Trim();
        rule.SubjectTemplate = request.SubjectTemplate.Trim();
        rule.BodyTemplate = request.BodyTemplate.Trim();
        rule.Priority = request.Priority;
        rule.Enabled = request.Enabled;
        rule.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(rule.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await db.NotificationRules
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (rule is null) return NotFound();

        db.NotificationRules.Remove(rule);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
