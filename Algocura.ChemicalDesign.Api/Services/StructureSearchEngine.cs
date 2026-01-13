using Algocura.ChemicalDesign.Api.Domain;

namespace Algocura.ChemicalDesign.Api.Services;

public interface IStructureSearchEngine
{
    IReadOnlyCollection<StructureSearchMatch> Search(
        QueryStructure query, StructureSearchType searchType,
        decimal similarityThreshold, int maximumResults,
        IReadOnlyCollection<ChemicalLibraryEntry> library);
}

public sealed record StructureSearchMatch(
    ChemicalLibraryEntry Entry,
    decimal MatchScore);

public sealed class DeterministicStructureSearchEngine : IStructureSearchEngine
{
    public IReadOnlyCollection<StructureSearchMatch> Search(
        QueryStructure query, StructureSearchType searchType,
        decimal similarityThreshold, int maximumResults,
        IReadOnlyCollection<ChemicalLibraryEntry> library)
    {
        if (similarityThreshold is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(similarityThreshold));

        var q = Normalize(query.Structure);
        return library
            .Select(x => new StructureSearchMatch(
                x, Score(q, Normalize(x.CanonicalSmiles), searchType)))
            .Where(x => searchType == StructureSearchType.Exact
                ? x.MatchScore == 1m
                : x.MatchScore >= similarityThreshold)
            .OrderByDescending(x => x.MatchScore)
            .ThenBy(x => x.Entry.Name)
            .Take(Math.Clamp(maximumResults, 1, 500))
            .ToArray();
    }

    private static decimal Score(string q, string c, StructureSearchType type)
    {
        if (q.Length == 0 || c.Length == 0) return 0m;
        if (type == StructureSearchType.Exact) return q == c ? 1m : 0m;
        if (type == StructureSearchType.Substructure && c.Contains(q)) return 1m;

        var left = Tokens(q);
        var right = Tokens(c);
        if (left.Count == 0 || right.Count == 0) return 0m;

        if (type == StructureSearchType.Substructure)
            return decimal.Round(left.Count(right.Contains) / (decimal)left.Count, 4);

        var union = left.Union(right).Count();
        return union == 0 ? 0m
            : decimal.Round(left.Intersect(right).Count() / (decimal)union, 4);
    }

    private static HashSet<string> Tokens(string value)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (value.Length < 2) { set.Add(value); return set; }
        for (var i = 0; i < value.Length - 1; i++)
            set.Add(value.Substring(i, 2));
        return set;
    }

    private static string Normalize(string value) =>
        new(value.Where(x => !char.IsWhiteSpace(x))
            .Select(char.ToUpperInvariant).ToArray());
}
