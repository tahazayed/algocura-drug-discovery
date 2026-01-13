using Algocura.LiteratureIntelligence.Api.Domain;
using System.Text.RegularExpressions;

namespace Algocura.LiteratureIntelligence.Api.Services;

public interface ILiteratureAnalysisService
{
    IReadOnlyCollection<EvidenceStatement> Analyze(
        Publication publication,
        string diseaseName,
        IReadOnlyCollection<string>? candidateGeneSymbols);
}

public sealed class RuleBasedLiteratureAnalysisService
    : ILiteratureAnalysisService
{
    public IReadOnlyCollection<EvidenceStatement> Analyze(
        Publication publication,
        string diseaseName,
        IReadOnlyCollection<string>? candidateGeneSymbols)
    {
        ArgumentNullException.ThrowIfNull(publication);

        var text = $"{publication.Title} {publication.Abstract}";

        var genes = candidateGeneSymbols?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        var results = new List<EvidenceStatement>();

        foreach (var gene in genes)
        {
            if (!Regex.IsMatch(
                text,
                $@"\b{Regex.Escape(gene)}\b",
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant))
            {
                continue;
            }

            var confidence = 0.65m;

            if (text.Contains(
                diseaseName,
                StringComparison.OrdinalIgnoreCase))
            {
                confidence += 0.15m;
            }

            if (text.Contains(
                    "associated",
                    StringComparison.OrdinalIgnoreCase) ||
                text.Contains(
                    "mutation",
                    StringComparison.OrdinalIgnoreCase) ||
                text.Contains(
                    "pathway",
                    StringComparison.OrdinalIgnoreCase))
            {
                confidence += 0.10m;
            }

            confidence = Math.Clamp(confidence, 0m, 0.95m);

            results.Add(
                new EvidenceStatement
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publication.Id,
                    DiseaseName = diseaseName.Trim(),
                    GeneSymbol = gene,
                    ProteinTarget = null,
                    RelationshipType =
                        InferRelationshipType(text),
                    Statement =
                        $"The publication mentions {gene} in the context of " +
                        $"{diseaseName}. Human scientific review is required.",
                    Strength = confidence switch
                    {
                        >= 0.85m => EvidenceStrength.VeryStrong,
                        >= 0.70m => EvidenceStrength.Strong,
                        >= 0.55m => EvidenceStrength.Moderate,
                        _ => EvidenceStrength.Low
                    },
                    ConfidenceScore = confidence,
                    HumanValidated = false,
                    CreatedAtUtc = DateTimeOffset.UtcNow
                });
        }

        return results;
    }

    private static string InferRelationshipType(string text)
    {
        if (text.Contains(
                "inhibit",
                StringComparison.OrdinalIgnoreCase) ||
            text.Contains(
                "suppression",
                StringComparison.OrdinalIgnoreCase))
        {
            return "NegativeModulation";
        }

        if (text.Contains(
                "activate",
                StringComparison.OrdinalIgnoreCase) ||
            text.Contains(
                "overexpression",
                StringComparison.OrdinalIgnoreCase))
        {
            return "PositiveModulation";
        }

        if (text.Contains(
                "mutation",
                StringComparison.OrdinalIgnoreCase) ||
            text.Contains(
                "variant",
                StringComparison.OrdinalIgnoreCase))
        {
            return "GeneticAssociation";
        }

        return "DiseaseAssociation";
    }
}
