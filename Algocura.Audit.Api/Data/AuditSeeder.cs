using Algocura.Audit.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Audit.Api.Data;

public static class AuditSeeder
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static async Task SeedAsync(
        AuditDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var records =
            new[]
            {
                Create(
                    "b1000000-0000-0000-0000-000000000001",
                    "Algocura.LiteratureIntelligence.Api",
                    "TargetHypothesis",
                    "13000000-0000-0000-0000-000000000001",
                    "Created",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000002",
                    "Algocura.TargetDiscovery.Api",
                    "TargetAssessment",
                    "21000000-0000-0000-0000-000000000001",
                    "Completed",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000003",
                    "Algocura.ChemicalDesign.Api",
                    "CandidateStructure",
                    "34000000-0000-0000-0000-000000000001",
                    "Updated",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000004",
                    "Algocura.CompoundCatalog.Api",
                    "Compound",
                    "7f06dc5b-379f-4589-9384-4f09c55f97c3",
                    "Created",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000005",
                    "Algocura.Screening.Api",
                    "ScreeningJob",
                    "41000000-0000-0000-0000-000000000001",
                    "Completed",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000006",
                    "Algocura.AssayManagement.Api",
                    "Assay",
                    "62000000-0000-0000-0000-000000000001",
                    "Published",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000007",
                    "Algocura.AIDiscovery.Api",
                    "PredictionJob",
                    "52000000-0000-0000-0000-000000000001",
                    "Completed",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000008",
                    "Algocura.Workflow.Api",
                    "DiscoveryWorkflow",
                    "71000000-0000-0000-0000-000000000001",
                    "Completed",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000009",
                    "Algocura.Notification.Api",
                    "Notification",
                    "82000000-0000-0000-0000-000000000003",
                    "Created",
                    "System"),

                Create(
                    "b1000000-0000-0000-0000-000000000010",
                    "Algocura.Reporting.Api",
                    "ReportRun",
                    "a2000000-0000-0000-0000-000000000001",
                    "Completed",
                    "System")
            };

        var ids =
            records
                .Select(x => x.EventId)
                .ToArray();

        var existing =
            await dbContext.AuditRecords
                .AsNoTracking()
                .Where(x => ids.Contains(x.EventId))
                .Select(x => x.EventId)
                .ToListAsync(cancellationToken);

        var existingSet =
            existing.ToHashSet();

        var missing =
            records
                .Where(x =>
                    !existingSet.Contains(x.EventId))
                .ToArray();

        if (missing.Length == 0)
        {
            return;
        }

        await dbContext.AuditRecords.AddRangeAsync(
            missing,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static AuditRecord Create(
        string eventId,
        string serviceName,
        string entityType,
        string entityId,
        string action,
        string actorType)
    {
        return new AuditRecord
        {
            EventId =
                Guid.Parse(eventId),

            OccurredAtUtc =
                SeedTimestamp,

            ServiceName =
                serviceName,

            EntityType =
                entityType,

            EntityId =
                entityId,

            Action =
                action,

            UserId =
                SystemUsers.SystemUserId,

            Subject =
                "system",

            UserDisplayName =
                "Algocura System",

            UserEmail =
                null,

            ActorType =
                actorType,

            PreviousValuesJson =
                null,

            CurrentValuesJson =
                "{\"seeded\":true}",

            ChangedPropertiesJson =
                "[\"seeded\"]",

            CorrelationId =
                "prototype-trem2-correlation",

            WorkflowId =
                "71000000-0000-0000-0000-000000000001",

            TraceId =
                "seed-trace",

            IpAddress =
                null,

            UserAgent =
                "Algocura.Audit.Api.Seeder",

            EventType =
                "EntityAuditEvent",

            EventVersion =
                "1.0",

            ReceivedAtUtc =
                SeedTimestamp,

            CreatedByUserId =
                SystemUsers.SystemUserId,

            UpdatedByUserId =
                SystemUsers.SystemUserId,

            CreatedAtUtc =
                SeedTimestamp,

            UpdatedAtUtc =
                SeedTimestamp
        };
    }
}
