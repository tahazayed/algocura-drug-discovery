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

public sealed class ResearchSubjectsControllerTests
{
    [Fact]
    public async Task Import_UpsertsSubjectsAndPublishesSummaryEvent()
    {
        await using var database = TestDatabase.Create();
        var dataset = new ClinicalDataset
        {
            Id = Guid.NewGuid(), Name = "Cancer Cohort", SourceName = "Public Source", StudyCode = "STUDY-1",
            Status = ClinicalDatasetStatus.Draft, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        database.Context.ClinicalDatasets.Add(dataset);
        await database.Context.SaveChangesAsync();

        var publisher = Substitute.For<IEventPublisher>();
        var sut = new ResearchSubjectsController(database.Context, publisher, NullLogger<ResearchSubjectsController>.Instance);
        var request = new ImportResearchSubjectsRequest
        {
            Subjects =
            [
                new ResearchSubjectImportItem { ExternalSubjectId = "CASE-001", AgeAtDiagnosis = 63, Sex = "Female", PrimarySite = "Lung", DiseaseStage = "Stage IIIA", Diagnosis = "Lung adenocarcinoma" },
                new ResearchSubjectImportItem { ExternalSubjectId = "CASE-002", AgeAtDiagnosis = 55, Sex = "Male", PrimarySite = "Lung", DiseaseStage = "Stage II" }
            ]
        };

        var action = await sut.Import(dataset.Id, request, CancellationToken.None);
        var ok = action.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ImportResearchSubjectsResponse>().Subject;
        response.InsertedCount.Should().Be(2);
        response.UpdatedCount.Should().Be(0);
        response.TotalSubjectCount.Should().Be(2);

        var storedDataset = await database.Context.ClinicalDatasets.FindAsync(dataset.Id);
        storedDataset!.Status.Should().Be(ClinicalDatasetStatus.Ready);

        await publisher.Received(1).PublishAsync(
            Arg.Is<ResearchSubjectsImported>(e => e.ClinicalDatasetId == dataset.Id && e.InsertedCount == 2 && e.TotalSubjectCount == 2),
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Import_ReimportSameExternalId_UpdatesInsteadOfDuplicating()
    {
        await using var database = TestDatabase.Create();
        var dataset = new ClinicalDataset
        {
            Id = Guid.NewGuid(), Name = "Cancer Cohort", SourceName = "Public Source",
            Status = ClinicalDatasetStatus.Ready, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        database.Context.ClinicalDatasets.Add(dataset);
        database.Context.ResearchSubjects.Add(new ResearchSubject
        {
            Id = Guid.NewGuid(), ClinicalDatasetId = dataset.Id, ExternalSubjectId = "CASE-001", AgeAtDiagnosis = 50,
            ImportedAtUtc = DateTimeOffset.UtcNow, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        await database.Context.SaveChangesAsync();

        var sut = new ResearchSubjectsController(database.Context, Substitute.For<IEventPublisher>(), NullLogger<ResearchSubjectsController>.Instance);
        var request = new ImportResearchSubjectsRequest { Subjects = [new ResearchSubjectImportItem { ExternalSubjectId = "CASE-001", AgeAtDiagnosis = 64, Diagnosis = "Updated diagnosis" }] };
        var action = await sut.Import(dataset.Id, request, CancellationToken.None);
        var ok = action.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ImportResearchSubjectsResponse>().Subject;
        response.InsertedCount.Should().Be(0);
        response.UpdatedCount.Should().Be(1);
        database.Context.ResearchSubjects.Should().ContainSingle(x => x.ExternalSubjectId == "CASE-001" && x.AgeAtDiagnosis == 64);
    }
}
