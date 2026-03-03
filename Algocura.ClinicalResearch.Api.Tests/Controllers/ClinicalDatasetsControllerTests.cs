using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Controllers;
using Algocura.ClinicalResearch.Api.Domain;
using Algocura.ClinicalResearch.Api.Events;
using Algocura.ClinicalResearch.Api.Tests.Support;
using Algocura.Platform.Messaging.Publishing;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.ClinicalResearch.Api.Tests.Controllers;

public sealed class ClinicalDatasetsControllerTests
{
    [Fact]
    public async Task Create_RegistersDatasetAndPublishesEvent()
    {
        await using var database = TestDatabase.Create();
        var publisher = Substitute.For<IEventPublisher>();
        var sut = new ClinicalDatasetsController(database.Context, publisher, NullLogger<ClinicalDatasetsController>.Instance);

        var action = await sut.Create(new CreateClinicalDatasetRequest
        {
            Name = "Public Cancer Cohort",
            SourceName = "NCI GDC",
            SourceUri = "https://portal.gdc.cancer.gov/",
            StudyCode = "DEMO-STUDY",
            DiseaseArea = "Oncology"
        }, CancellationToken.None);

        var result = action.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = result.Value.Should().BeOfType<ClinicalDatasetResponse>().Subject;
        response.Status.Should().Be(ClinicalDatasetStatus.Draft);
        response.SubjectCount.Should().Be(0);

        await publisher.Received(1).PublishAsync(
            Arg.Is<ClinicalDatasetRegistered>(e => e.ClinicalDatasetId == response.Id && e.SourceName == "NCI GDC"),
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
