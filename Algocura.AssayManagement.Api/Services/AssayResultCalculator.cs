using Algocura.AssayManagement.Api.Domain;

namespace Algocura.AssayManagement.Api.Services;

public interface IAssayResultCalculator
{
    IReadOnlyCollection<AssayResult> Calculate(
        Assay assay,
        IReadOnlyCollection<AssayMeasurement> measurements);
}

public sealed class DeterministicAssayResultCalculator
    : IAssayResultCalculator
{
    public IReadOnlyCollection<AssayResult> Calculate(
        Assay assay,
        IReadOnlyCollection<AssayMeasurement> measurements)
    {
        ArgumentNullException.ThrowIfNull(assay);
        ArgumentNullException.ThrowIfNull(measurements);

        var now = DateTimeOffset.UtcNow;

        return measurements
            .Where(x => !x.IsControl)
            .GroupBy(x => x.CompoundId)
            .Select(group =>
            {
                var values =
                    group.Select(x => x.Value)
                        .ToArray();

                var mean =
                    values.Average();

                var standardDeviation =
                    CalculateStandardDeviation(values);

                var confidence =
                    CalculateConfidence(
                        values.Length,
                        standardDeviation);

                var outcome =
                    DetermineOutcome(
                        assay,
                        mean);

                return new AssayResult
                {
                    Id = Guid.NewGuid(),
                    AssayId = assay.Id,
                    CompoundId = group.Key,
                    MeanValue =
                        decimal.Round(mean, 6),
                    StandardDeviation =
                        decimal.Round(standardDeviation, 6),
                    ReplicateCount =
                        values.Length,
                    ConfidenceScore =
                        confidence,
                    Outcome =
                        outcome,
                    SelectedForAi =
                        outcome is AssayOutcome.Active or
                        AssayOutcome.Potent,
                    CreatedAtUtc = now
                };
            })
            .ToArray();
    }

    private static decimal CalculateStandardDeviation(
        IReadOnlyCollection<decimal> values)
    {
        if (values.Count <= 1)
        {
            return 0m;
        }

        var average = values.Average();

        var variance =
            values.Sum(value =>
            {
                var difference =
                    value - average;

                return difference * difference;
            }) / (values.Count - 1);

        return (decimal)Math.Sqrt((double)variance);
    }

    private static decimal CalculateConfidence(
        int replicateCount,
        decimal standardDeviation)
    {
        var replicateContribution =
            Math.Min(replicateCount / 3m, 1m) * 0.60m;

        var variabilityContribution =
            Math.Max(
                0m,
                1m - standardDeviation / 100m) * 0.40m;

        return decimal.Round(
            Math.Clamp(
                replicateContribution +
                variabilityContribution,
                0m,
                1m),
            4);
    }

    private static AssayOutcome DetermineOutcome(
        Assay assay,
        decimal mean)
    {
        if (assay.AssayType ==
            AssayType.Toxicity)
        {
            return mean >= assay.ActivityThreshold
                ? AssayOutcome.Toxic
                : AssayOutcome.Inactive;
        }

        if (mean >= assay.ActivityThreshold * 1.25m)
        {
            return AssayOutcome.Potent;
        }

        if (mean >= assay.ActivityThreshold)
        {
            return AssayOutcome.Active;
        }

        return AssayOutcome.Inactive;
    }
}

