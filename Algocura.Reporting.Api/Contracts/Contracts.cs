using Algocura.Reporting.Api.Domain;

namespace Algocura.Reporting.Api.Contracts;

public sealed record CreateReportDefinitionRequest(
    string Name,
    string? Description,
    string ReportType,
    string? DefaultParametersJson);

public sealed record UpdateReportDefinitionRequest(
    string Name,
    string? Description,
    string ReportType,
    string? DefaultParametersJson,
    ReportDefinitionStatus Status);

public sealed record RunReportRequest(
    string? Name,
    string? ParametersJson);

public sealed record ReportDefinitionResponse(
    Guid Id,
    string Name,
    string? Description,
    string ReportType,
    ReportDefinitionStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ReportMetricResponse(
    string Name,
    decimal Value,
    MetricValueType ValueType,
    string? Unit);

public sealed record ReportRunResponse(
    Guid Id,
    Guid ReportDefinitionId,
    string Name,
    ReportRunStatus Status,
    string? FailureReason,
    string? ResultLocation,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyCollection<ReportMetricResponse> Metrics);

public sealed record ReportingSummaryResponse(
    int TotalProjections,
    IReadOnlyDictionary<string, int> ProjectionCounts,
    int ActiveReportDefinitions,
    int CompletedReportRuns);
