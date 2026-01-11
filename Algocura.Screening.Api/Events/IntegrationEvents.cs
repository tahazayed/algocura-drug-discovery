namespace Algocura.Screening.Api.Events;

public abstract record IntegrationEvent
{
    protected IntegrationEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc)
    {
        EventId = eventId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public abstract string EventType { get; }

    public virtual string EventVersion => "1.0";

    public virtual string Source => "Algocura.Screening.Api";
}

public sealed record ScreeningCreated : IntegrationEvent
{
    public ScreeningCreated(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        Guid screeningId,
        string name,
        string screeningMethod,
        int candidateCount)
        : base(eventId, occurredAtUtc)
    {
        ScreeningId = screeningId;
        Name = name;
        ScreeningMethod = screeningMethod;
        CandidateCount = candidateCount;
    }

    public override string EventType => nameof(ScreeningCreated);

    public Guid ScreeningId { get; init; }

    public string Name { get; init; }

    public string ScreeningMethod { get; init; }

    public int CandidateCount { get; init; }
}

public sealed record ScreeningStarted : IntegrationEvent
{
    public ScreeningStarted(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        Guid screeningId)
        : base(eventId, occurredAtUtc)
    {
        ScreeningId = screeningId;
    }

    public override string EventType => nameof(ScreeningStarted);

    public Guid ScreeningId { get; init; }
}

public sealed record ScreeningCompleted : IntegrationEvent
{
    public ScreeningCompleted(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        Guid screeningId,
        int resultCount,
        int selectedCount)
        : base(eventId, occurredAtUtc)
    {
        ScreeningId = screeningId;
        ResultCount = resultCount;
        SelectedCount = selectedCount;
    }

    public override string EventType => nameof(ScreeningCompleted);

    public Guid ScreeningId { get; init; }

    public int ResultCount { get; init; }

    public int SelectedCount { get; init; }
}

public sealed record ScreeningFailed : IntegrationEvent
{
    public ScreeningFailed(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        Guid screeningId,
        string reason)
        : base(eventId, occurredAtUtc)
    {
        ScreeningId = screeningId;
        Reason = reason;
    }

    public override string EventType => nameof(ScreeningFailed);

    public Guid ScreeningId { get; init; }

    public string Reason { get; init; }
}

public sealed record ScreeningCancelled : IntegrationEvent
{
    public ScreeningCancelled(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        Guid screeningId)
        : base(eventId, occurredAtUtc)
    {
        ScreeningId = screeningId;
    }

    public override string EventType => nameof(ScreeningCancelled);

    public Guid ScreeningId { get; init; }
}
