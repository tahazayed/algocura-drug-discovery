using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Data;
using Algocura.ClinicalResearch.Api.Domain;
using Algocura.ClinicalResearch.Api.Events;
using Algocura.ClinicalResearch.Api.Mapping;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ClinicalResearch.Api.Controllers;

[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/research-subjects")]
[Produces("application/json")]
public sealed class ResearchSubjectsController(
    ClinicalResearchDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<ResearchSubjectsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ResearchSubjectResponse>>> GetAll(
        [FromQuery] Guid? datasetId,
        [FromQuery] string? search,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 500);
        var query = dbContext.ResearchSubjects.AsNoTracking().AsQueryable();
        if (datasetId.HasValue) query = query.Where(x => x.ClinicalDatasetId == datasetId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.ExternalSubjectId.ToLower().Contains(term) ||
                                     (x.Diagnosis != null && x.Diagnosis.ToLower().Contains(term)) ||
                                     (x.PrimarySite != null && x.PrimarySite.ToLower().Contains(term)));
        }

        var rows = await query.OrderBy(x => x.ExternalSubjectId).Skip(skip).Take(take)
            .Select(x => new ResearchSubjectResponse(
                x.Id, x.ClinicalDatasetId, x.ExternalSubjectId, x.AgeAtDiagnosis,
                x.Sex, x.PrimarySite, x.DiseaseStage, x.VitalStatus,
                x.Diagnosis, x.TreatmentSummary, x.MolecularFeaturesJson,
                x.AdditionalAttributesJson, x.ImportedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPost("datasets/{datasetId:guid}/import")]
    public async Task<ActionResult<ImportResearchSubjectsResponse>> Import(
        Guid datasetId,
        ImportResearchSubjectsRequest request,
        CancellationToken cancellationToken)
    {
        var dataset = await dbContext.ClinicalDatasets.SingleOrDefaultAsync(x => x.Id == datasetId, cancellationToken);
        if (dataset is null) return Problem(statusCode: 404, title: "Clinical dataset not found");

        var normalizedItems = request.Subjects
            .GroupBy(x => x.ExternalSubjectId.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Last())
            .ToList();

        var externalIds = normalizedItems.Select(x => x.ExternalSubjectId.Trim()).ToList();
        var existing = await dbContext.ResearchSubjects
            .Where(x => x.ClinicalDatasetId == datasetId && externalIds.Contains(x.ExternalSubjectId))
            .ToDictionaryAsync(x => x.ExternalSubjectId, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var inserted = 0;
        var updated = 0;

        foreach (var item in normalizedItems)
        {
            var externalId = item.ExternalSubjectId.Trim();
            if (!existing.TryGetValue(externalId, out var subject))
            {
                subject = new ResearchSubject
                {
                    Id = Guid.NewGuid(), ClinicalDatasetId = datasetId,
                    ExternalSubjectId = externalId, CreatedAtUtc = now, ImportedAtUtc = now, UpdatedAtUtc = now
                };
                dbContext.ResearchSubjects.Add(subject);
                inserted++;
            }
            else
            {
                updated++;
                subject.ImportedAtUtc = now;
                subject.UpdatedAtUtc = now;
            }

            subject.AgeAtDiagnosis = item.AgeAtDiagnosis;
            subject.Sex = item.Sex?.Trim();
            subject.PrimarySite = item.PrimarySite?.Trim();
            subject.DiseaseStage = item.DiseaseStage?.Trim();
            subject.VitalStatus = item.VitalStatus?.Trim();
            subject.Diagnosis = item.Diagnosis?.Trim();
            subject.TreatmentSummary = item.TreatmentSummary?.Trim();
            subject.MolecularFeaturesJson = item.MolecularFeaturesJson?.Trim();
            subject.AdditionalAttributesJson = item.AdditionalAttributesJson?.Trim();
        }

        dataset.UpdatedAtUtc = now;
        if (dataset.Status == ClinicalDatasetStatus.Draft && normalizedItems.Count > 0)
            dataset.Status = ClinicalDatasetStatus.Ready;

        await dbContext.SaveChangesAsync(cancellationToken);
        var total = await dbContext.ResearchSubjects.CountAsync(x => x.ClinicalDatasetId == datasetId, cancellationToken);

        await eventPublisher.PublishAsync(new ResearchSubjectsImported
        {
            ClinicalDatasetId = dataset.Id,
            DatasetName = dataset.Name,
            SourceName = dataset.SourceName,
            StudyCode = dataset.StudyCode,
            InsertedCount = inserted,
            UpdatedCount = updated,
            TotalSubjectCount = total
        }, HttpContext?.TraceIdentifier, cancellationToken);

        logger.LogInformation("Imported {InsertedCount} new and {UpdatedCount} updated research subjects into dataset {ClinicalDatasetId}", inserted, updated, datasetId);
        return Ok(new ImportResearchSubjectsResponse(datasetId, inserted, updated, total, now));
    }
}
