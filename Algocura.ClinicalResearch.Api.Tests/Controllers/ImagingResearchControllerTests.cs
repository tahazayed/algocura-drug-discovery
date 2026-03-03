using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Controllers;
using Algocura.ClinicalResearch.Api.Events;
using Algocura.ClinicalResearch.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Algocura.ClinicalResearch.Api.Tests.Controllers;

public sealed class ImagingResearchControllerTests
{
    [Fact]
    public async Task Curate_FiltersToCtAndMinimumImageCount()
    {
        var tcia = Substitute.For<ITciaNbiaClient>();
        tcia.GetSeriesAsync("Demo", "P-1", Arg.Any<CancellationToken>()).Returns([
            new TciaSeriesResponse("Demo","P-1","study","ct-1","CT","CHEST","Baseline",120,"20260101"),
            new TciaSeriesResponse("Demo","P-1","study","mr-1","MR","CHEST","MRI",90,"20260101"),
            new TciaSeriesResponse("Demo","P-1","study","ct-2","CT","CHEST","Scout",1,"20260101")
        ]);
        var publisher = Substitute.For<IEventPublisher>();
        var sut = Create(tcia, publisher);

        var action = await sut.CurateTciaSeries(new CuratedImagingSeriesRequest
        {
            Collection = "Demo", PatientId = "P-1", Modality = "CT", BodyPartContains = "CHEST", MinimumImageCount = 10
        }, CancellationToken.None);

        var ok = action.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<CuratedImagingSeriesResponse>().Subject;
        response.RetrievedCount.Should().Be(3);
        response.CuratedCount.Should().Be(1);
        response.Series.Single().SeriesInstanceUid.Should().Be("ct-1");
        await publisher.Received(1).PublishAsync(Arg.Any<ImagingSeriesRetrieved>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EstimateResponse_ReturnsResearchOnlyScore()
    {
        var publisher = Substitute.For<IEventPublisher>();
        var sut = Create(Substitute.For<ITciaNbiaClient>(), publisher);
        var action = await sut.EstimateResponse(new ImmunotherapyResponseEstimateRequest
        {
            Age=61, NormalizedPdL1=.8, NormalizedTmb=.7, NormalizedImmuneInfiltration=.6,
            NormalizedTumorBurden=.4, PriorTreatmentBurden=.2, CancerType="Lung"
        }, CancellationToken.None);
        var ok = action.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ImmunotherapyResponseEstimateResponse>().Subject;
        response.ResearchResponseScore.Should().BeInRange(0,1);
        response.Disclaimer.Should().Contain("not clinically fitted");
    }

    [Fact]
    public async Task Simulator_ProducesDifferentPngAndShrinksPrototypeRoi()
    {
        var simulator = new PrototypeFollowUpCtSimulator();
        using var image = new Image<Rgba32>(128, 128, new Rgba32(90,90,90));
        for (var y=44; y<=84; y++) for (var x=44; x<=84; x++) image[x,y]=new Rgba32(180,180,180);
        await using var ms = new MemoryStream();
        await image.SaveAsPngAsync(ms);
        ms.Position=0;
        var result=await simulator.SimulateAsync(ms,.8,64,64,20,CancellationToken.None);
        result.Png.Should().NotBeEmpty();
        result.Metadata.SimulatedLesionRadiusPixels.Should().BeLessThan(20);
        result.Metadata.Disclaimer.Should().Contain("Synthetic research visualization");
    }

    [Fact]
    public async Task GetTciaPatients_returns_discovered_patients()
    {
        var tcia = Substitute.For<ITciaNbiaClient>();
        var publisher = Substitute.For<IEventPublisher>();

        tcia.GetPatientsAsync("NSCLC-Radiomics", "CT", Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new TciaPatientResponse("NSCLC-Radiomics", "LUNG1-001")
            });

        var sut = Create(tcia, publisher);

        var result = await sut.GetTciaPatients(
            "NSCLC-Radiomics",
            "CT",
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var patients = Assert.IsAssignableFrom<IReadOnlyCollection<TciaPatientResponse>>(ok.Value);
        Assert.Single(patients);
        Assert.Equal("LUNG1-001", patients.First().PatientId);
    }

    private static ImagingResearchController Create(ITciaNbiaClient tcia, IEventPublisher publisher) =>
        new(tcia, new PrototypeImmunotherapyResponseEstimator(), new PrototypeFollowUpCtSimulator(), publisher, NullLogger<ImagingResearchController>.Instance);
}
