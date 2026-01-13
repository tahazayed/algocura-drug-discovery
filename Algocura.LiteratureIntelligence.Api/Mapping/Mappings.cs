using Algocura.LiteratureIntelligence.Api.Contracts;
using Algocura.LiteratureIntelligence.Api.Domain;

namespace Algocura.LiteratureIntelligence.Api.Mapping;

public static class Mappings
{
    public static EvidenceStatementResponse ToResponse(
        this EvidenceStatement evidence)
    {
        return new EvidenceStatementResponse(
            evidence.Id,
            evidence.PublicationId,
            evidence.DiseaseName,
            evidence.GeneSymbol,
            evidence.ProteinTarget,
            evidence.RelationshipType,
            evidence.Statement,
            evidence.Strength,
            evidence.ConfidenceScore,
            evidence.HumanValidated,
            evidence.CreatedAtUtc);
    }

    public static PublicationResponse ToResponse(
        this Publication publication)
    {
        return new PublicationResponse(
            publication.Id,
            publication.Title,
            publication.Abstract,
            publication.Journal,
            publication.ExternalId,
            publication.SourceUrl,
            publication.PublishedDate,
            publication.Status,
            publication.CreatedAtUtc,
            publication.UpdatedAtUtc,
            publication.AnalyzedAtUtc,
            publication.Authors
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Name)
                .ToArray(),
            publication.EvidenceStatements
                .OrderByDescending(x => x.ConfidenceScore)
                .Select(x => x.ToResponse())
                .ToArray());
    }

    public static TargetHypothesisResponse ToResponse(
        this TargetHypothesis hypothesis)
    {
        return new TargetHypothesisResponse(
            hypothesis.Id,
            hypothesis.DiseaseName,
            hypothesis.GeneSymbol,
            hypothesis.ProteinTarget,
            hypothesis.DesiredModulation,
            hypothesis.Rationale,
            hypothesis.EvidenceScore,
            hypothesis.Status,
            hypothesis.CreatedAtUtc,
            hypothesis.UpdatedAtUtc,
            hypothesis.EvidenceLinks
                .Select(x => x.EvidenceStatementId)
                .ToArray());
    }
}
