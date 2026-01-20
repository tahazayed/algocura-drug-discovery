using Algocura.Platform.Messaging.Publishing;
using Algocura.TargetDiscovery.Api.Contracts;
using Algocura.TargetDiscovery.Api.Data;
using Algocura.TargetDiscovery.Api.Domain;
using Algocura.TargetDiscovery.Api.Events;
using Algocura.TargetDiscovery.Api.Mapping;
using Algocura.TargetDiscovery.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.TargetDiscovery.Api.Controllers;

[ApiController]
[Route("api/v1/target-assessments")]
public sealed class TargetAssessmentsController(
    TargetDiscoveryDbContext dbContext,
    ITargetAssessmentScoringEngine scoringEngine,
    IEventPublisher eventPublisher,
    ILogger<TargetAssessmentsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<TargetAssessmentResponse>>> GetAll(
            TargetAssessmentStatus? status,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 200);

        var query = dbContext.TargetAssessments
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(
                x => x.Status == status.Value);
        }

        var assessments = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Ok(
            assessments
                .Select(x => x.ToResponse())
                .ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TargetAssessmentResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var assessment = await dbContext.TargetAssessments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        return assessment is null
            ? NotFound()
            : Ok(assessment.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<TargetAssessmentResponse>> Create(
        CreateTargetAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var hypothesis = await dbContext.TargetHypotheses
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.TargetHypothesisId ==
                    request.TargetHypothesisId,
                cancellationToken);

        if (hypothesis is null)
        {
            ModelState.AddModelError(
                nameof(request.TargetHypothesisId),
                "The target hypothesis was not found.");

            return BadRequest(
                new ValidationProblemDetails(ModelState));
        }

        if (!ScoresAreValid(
            request.GeneticEvidenceScore,
            request.BiologicalPlausibilityScore,
            request.DruggabilityScore,
            request.SafetyRiskScore,
            request.TranslationalPotentialScore))
        {
            ModelState.AddModelError(
                "Scores",
                "All assessment scores must be between 0 and 1.");

            return BadRequest(
                new ValidationProblemDetails(ModelState));
        }

        var now = DateTimeOffset.UtcNow;

        var assessment = new TargetAssessment
        {
            Id = Guid.NewGuid(),
            TargetHypothesisId =
                hypothesis.TargetHypothesisId,
            Name = request.Name.Trim(),
            ScientificRationale =
                request.ScientificRationale?.Trim(),
            LiteratureEvidenceScore =
                hypothesis.LiteratureEvidenceScore,
            GeneticEvidenceScore =
                request.GeneticEvidenceScore,
            BiologicalPlausibilityScore =
                request.BiologicalPlausibilityScore,
            DruggabilityScore =
                request.DruggabilityScore,
            SafetyRiskScore =
                request.SafetyRiskScore,
            TranslationalPotentialScore =
                request.TranslationalPotentialScore,
            OverallScore = 0m,
            ConfidenceLevel =
                TargetConfidenceLevel.Low,
            Status =
                TargetAssessmentStatus.Draft,
            ReviewNotes =
                request.ReviewNotes?.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.TargetAssessments.Add(assessment);

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new TargetAssessmentCreated(
                Guid.NewGuid(),
                now,
                assessment.Id,
                hypothesis.TargetHypothesisId,
                hypothesis.DiseaseName,
                hypothesis.GeneSymbol),
            correlationId: assessment.Id.ToString(),
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Created target assessment {TargetAssessmentId}.",
            assessment.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = assessment.Id },
            assessment.ToResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TargetAssessmentResponse>> Update(
        Guid id,
        UpdateTargetAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var assessment = await dbContext.TargetAssessments
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (assessment is null)
        {
            return NotFound();
        }

        if (assessment.Status is
            TargetAssessmentStatus.Completed or
            TargetAssessmentStatus.Selected)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Assessment is finalized",
                    Detail =
                        "Completed or selected assessments cannot be edited.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        if (!ScoresAreValid(
            request.GeneticEvidenceScore,
            request.BiologicalPlausibilityScore,
            request.DruggabilityScore,
            request.SafetyRiskScore,
            request.TranslationalPotentialScore))
        {
            ModelState.AddModelError(
                "Scores",
                "All assessment scores must be between 0 and 1.");

            return BadRequest(
                new ValidationProblemDetails(ModelState));
        }

        assessment.GeneticEvidenceScore =
            request.GeneticEvidenceScore;

        assessment.BiologicalPlausibilityScore =
            request.BiologicalPlausibilityScore;

        assessment.DruggabilityScore =
            request.DruggabilityScore;

        assessment.SafetyRiskScore =
            request.SafetyRiskScore;

        assessment.TranslationalPotentialScore =
            request.TranslationalPotentialScore;

        assessment.ScientificRationale =
            request.ScientificRationale?.Trim();

        assessment.ReviewNotes =
            request.ReviewNotes?.Trim();

        assessment.Status =
            TargetAssessmentStatus.InProgress;

        assessment.UpdatedAtUtc =
            DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(assessment.ToResponse());
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<TargetAssessmentResponse>> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var assessment = await dbContext.TargetAssessments
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (assessment is null)
        {
            return NotFound();
        }

        if (assessment.Status is
            TargetAssessmentStatus.Completed or
            TargetAssessmentStatus.Selected)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Assessment is already finalized",
                    Detail =
                        "The assessment has already been completed or selected.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        var result =
            scoringEngine.Calculate(assessment);

        var now = DateTimeOffset.UtcNow;

        assessment.OverallScore =
            result.OverallScore;

        assessment.ConfidenceLevel =
            result.ConfidenceLevel;

        assessment.Status =
            TargetAssessmentStatus.Completed;

        assessment.CompletedAtUtc = now;
        assessment.UpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new TargetAssessmentCompleted(
                Guid.NewGuid(),
                now,
                assessment.Id,
                assessment.TargetHypothesisId,
                assessment.OverallScore,
                assessment.ConfidenceLevel.ToString()),
            correlationId: assessment.Id.ToString(),
            cancellationToken: cancellationToken);

        return Ok(assessment.ToResponse());
    }

    [HttpPost("{id:guid}/select")]
    public async Task<ActionResult<TargetAssessmentResponse>> Select(
        Guid id,
        CancellationToken cancellationToken)
    {
        var assessment = await dbContext.TargetAssessments
            .Include(x => x.TargetHypothesis)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (assessment is null)
        {
            return NotFound();
        }

        if (assessment.Status !=
            TargetAssessmentStatus.Completed)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Assessment is not completed",
                    Detail =
                        "Only completed assessments can be selected.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        var hypothesis = assessment.TargetHypothesis
            ?? throw new InvalidOperationException(
                "The target hypothesis could not be loaded.");

        var now = DateTimeOffset.UtcNow;

        assessment.Status =
            TargetAssessmentStatus.Selected;

        assessment.SelectedAtUtc = now;
        assessment.UpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new TargetSelected(
                Guid.NewGuid(),
                now,
                assessment.Id,
                hypothesis.TargetHypothesisId,
                hypothesis.DiseaseName,
                hypothesis.GeneSymbol,
                hypothesis.ProteinTarget,
                hypothesis.DesiredModulation,
                assessment.OverallScore),
            correlationId: assessment.Id.ToString(),
            cancellationToken: cancellationToken);

        return Ok(assessment.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var assessment = await dbContext.TargetAssessments
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (assessment is null)
        {
            return NotFound();
        }

        if (assessment.Status ==
            TargetAssessmentStatus.Selected)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Selected assessment",
                    Detail =
                        "A selected target assessment cannot be deleted.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        dbContext.TargetAssessments.Remove(assessment);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static bool ScoresAreValid(
        params decimal[] scores)
    {
        return scores.All(
            score => score is >= 0m and <= 1m);
    }
}
