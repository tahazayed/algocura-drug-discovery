using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Controllers;
using Algocura.ClinicalResearch.Api.Events;
using Algocura.ClinicalResearch.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Algocura.ClinicalResearch.Api.Tests.Controllers;

public sealed class PublicHealthResearchControllerTests
{
    [Fact]
    public async Task AnalyzeCovid19_ReturnsAnalysisAndPublishesEvent()
    {
        var service = Substitute.For<IWaelCovidResearchService>();
        var publisher = Substitute.For<IEventPublisher>();
        var controller = new PublicHealthResearchController(service, publisher);
        var request = new Covid19PublicHealthAnalysisRequest();
        var response = new Covid19PublicHealthAnalysisResponse(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "US",
            request.StartDate,
            request.EndDate,
            1,
            2,
            50,
            new Covid19CountryContext("US", 100, 5, 1000, 20, 0.1, 0.05),
            [],
            [],
            new Covid19SimulationParameters(250, 20260907, 500, 100000, 0.02, 0.5, 10, 50),
            "test",
            "research only");

        service.AnalyzeAsync(request, Arg.Any<CancellationToken>()).Returns(response);

        var result = await controller.AnalyzeCovid19(request, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(response);
        await publisher.Received(1).PublishAsync(
            Arg.Is<Covid19PublicHealthAnalysisCompleted>(x => x.AnalysisId == response.AnalysisId),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
