using Algocura.Search.Api.Domain;
using System.Text.Json;

namespace Algocura.Search.Api.Services;

public sealed record PlatformSearchEvent(
    Guid EventId,
    string EventType,
    string Source,
    string? CorrelationId,
    DateTimeOffset OccurredAtUtc,
    string Payload);

public static class SearchDocumentFactory
{
    public static SearchDocument? Create(
        PlatformSearchEvent platformEvent)
    {
        using var json =
            JsonDocument.Parse(platformEvent.Payload);

        var root = json.RootElement;
        var type = MapDocumentType(platformEvent.EventType);

        if (type is null)
        {
            return null;
        }

        var sourceEntityId =
            ReadGuid(
                root,
                "compoundId",
                "screeningId",
                "assayId",
                "targetAssessmentId",
                "targetHypothesisId",
                "predictionId",
                "workflowId",
                "publicationId",
                "notificationId",
                "candidateStructureId")
            ?? platformEvent.EventId;

        var title =
            ReadString(
                root,
                "name",
                "title",
                "subject",
                "compoundName",
                "diseaseName")
            ?? $"{platformEvent.EventType} {sourceEntityId}";

        var now = DateTimeOffset.UtcNow;

        var document = new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceEntityId = sourceEntityId,
            SourceEventType = platformEvent.EventType,
            SourceService = platformEvent.Source,
            DocumentType = type.Value,
            Title = title,
            Summary = ReadString(
                root,
                "summary",
                "description",
                "rationale",
                "body",
                "objective"),
            ExternalReference = ReadString(
                root,
                "externalReference",
                "doi",
                "inchiKey"),
            CorrelationId = platformEvent.CorrelationId,
            WorkflowId = ReadString(root, "workflowId"),
            DiseaseName = ReadString(root, "diseaseName"),
            GeneSymbol = ReadString(root, "geneSymbol")?.ToUpperInvariant(),
            CompoundName = ReadString(root, "compoundName", "name"),
            InchiKey = ReadString(root, "inchiKey")?.ToUpperInvariant(),
            CanonicalSmiles = ReadString(root, "canonicalSmiles"),
            Outcome = ReadString(root, "outcome", "status"),
            Score = ReadDecimal(
                root,
                "score",
                "overallScore",
                "matchScore",
                "confidenceScore",
                "leadScore"),
            Status = SearchDocumentStatus.Active,
            SourceUpdatedAtUtc = platformEvent.OccurredAtUtc,
            IndexedAtUtc = now,
            UpdatedAtUtc = now
        };

        AddTag(document, "eventType", platformEvent.EventType);
        AddTag(document, "source", platformEvent.Source);

        if (!string.IsNullOrWhiteSpace(document.DiseaseName))
        {
            AddTag(document, "disease", document.DiseaseName);
        }

        if (!string.IsNullOrWhiteSpace(document.GeneSymbol))
        {
            AddTag(document, "gene", document.GeneSymbol);
        }

        return document;
    }

    private static SearchDocumentType? MapDocumentType(string eventType)
    {
        if (eventType.Contains("Publication", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("Literature", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.Publication;

        if (eventType.Contains("Target", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.Target;

        if (eventType.Contains("ChemicalDesign", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("StructureSearch", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.ChemicalDesign;

        if (eventType.Contains("Compound", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.Compound;

        if (eventType.Contains("Screening", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.Screening;

        if (eventType.Contains("Assay", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.Assay;

        if (eventType.Contains("Prediction", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("Lead", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.Prediction;

        if (eventType.Contains("Workflow", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.Workflow;

        if (eventType.Contains("Notification", StringComparison.OrdinalIgnoreCase))
            return SearchDocumentType.Notification;

        return null;
    }

    private static Guid? ReadGuid(
        JsonElement root,
        params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(root, name, out var property) &&
                Guid.TryParse(property.ToString(), out var value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? ReadString(
        JsonElement root,
        params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(root, name, out var property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                var value = property.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }
        }

        return null;
    }

    private static decimal? ReadDecimal(
        JsonElement root,
        params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(root, name, out var property) &&
                decimal.TryParse(property.ToString(), out var value))
            {
                return value;
            }
        }

        return null;
    }

    private static bool TryGetProperty(
        JsonElement root,
        string name,
        out JsonElement value)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(
                    property.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static void AddTag(
        SearchDocument document,
        string name,
        string value)
    {
        document.Tags.Add(
            new SearchDocumentTag
            {
                Id = Guid.NewGuid(),
                SearchDocumentId = document.Id,
                Name = name,
                Value = value
            });
    }
}

