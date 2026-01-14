using Algocura.AssayManagement.Api.Domain;
using Algocura.AssayManagement.Api.Services;
using FluentAssertions;

namespace Algocura.AssayManagement.Api.Tests.Services;

public sealed class AssayResultCalculatorTests
{
    [Fact]
    public void Calculate_Replicates_CreatesCompoundResult()
    {
        var assay = new Assay
        {
            Id = Guid.NewGuid(),
            ScreeningId = Guid.NewGuid(),
            Name = "Inhibition assay",
            AssayType = AssayType.Biochemical,
            MeasurementUnit = "Percent",
            ActivityThreshold = 50m,
            Status = AssayStatus.InProgress,
            QualityControlStatus = QualityControlStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var compoundId = Guid.NewGuid();

        var measurements = new[]
        {
            Measurement(assay.Id, compoundId, 1, 70m),
            Measurement(assay.Id, compoundId, 2, 74m),
            Measurement(assay.Id, compoundId, 3, 72m)
        };

        var sut =
            new DeterministicAssayResultCalculator();

        var results =
            sut.Calculate(
                assay,
                measurements);

        results.Should().ContainSingle();

        var result = results.Single();

        result.MeanValue.Should().Be(72m);
        result.ReplicateCount.Should().Be(3);
        result.Outcome.Should().Be(AssayOutcome.Potent);
        result.SelectedForAi.Should().BeTrue();
    }

    private static AssayMeasurement Measurement(
        Guid assayId,
        Guid compoundId,
        int replicate,
        decimal value)
    {
        return new AssayMeasurement
        {
            Id = Guid.NewGuid(),
            AssayId = assayId,
            CompoundId = compoundId,
            MeasurementType =
                MeasurementType.PercentInhibition,
            Concentration = 1m,
            ConcentrationUnit = "uM",
            ReplicateNumber = replicate,
            Value = value,
            RecordedAtUtc =
                DateTimeOffset.UtcNow
        };
    }
}

