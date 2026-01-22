using Algocura.CompoundCatalog.Api.Contracts;
using Algocura.CompoundCatalog.Api.Data;
using Algocura.CompoundCatalog.Api.Domain;
using Algocura.CompoundCatalog.Api.Events;
using Algocura.CompoundCatalog.Api.Mapping;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.CompoundCatalog.Api.Controllers;

/// <summary>
/// Manages compounds registered in the Algocura drug-discovery platform.
/// </summary>
[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/compounds")]
[Produces("application/json")]
public sealed class CompoundsController(
    CompoundCatalogDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<CompoundsController> logger)
    : ControllerBase
{
    /// <summary>
    /// Returns compounds using optional search and status filters.
    /// </summary>
    /// <param name="search">
    /// Optional text used to search compound names, InChIKeys, formulas, and SMILES.
    /// </param>
    /// <param name="status">Optional compound lifecycle status.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Maximum number of records to return.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [EndpointSummary("Search compounds")]
    [EndpointDescription(
        "Returns registered compounds and supports filtering by text and lifecycle status.")]
    [ProducesResponseType<IReadOnlyCollection<CompoundResponse>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CompoundResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] CompoundStatus? status,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 200);

        IQueryable<Compound> query = dbContext.Compounds
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(compound =>
                compound.Name.ToLower().Contains(normalizedSearch) ||
                compound.CanonicalSmiles.ToLower().Contains(normalizedSearch) ||
                (compound.InchiKey != null &&
                 compound.InchiKey.ToLower().Contains(normalizedSearch)) ||
                (compound.MolecularFormula != null &&
                 compound.MolecularFormula.ToLower().Contains(normalizedSearch)));
        }

        if (status.HasValue)
        {
            query = query.Where(compound => compound.Status == status.Value);
        }

        var compounds = await query
            .OrderBy(compound => compound.Name)
            .Skip(skip)
            .Take(take)
            .Select(compound => compound.ToResponse())
            .ToListAsync(cancellationToken);

        return Ok(compounds);
    }

    /// <summary>
    /// Returns one compound by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a compound")]
    [ProducesResponseType<CompoundResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompoundResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var compound = await dbContext.Compounds
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (compound is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Compound not found",
                detail: $"No compound with ID '{id}' was found.");
        }

        return Ok(compound.ToResponse());
    }

    /// <summary>
    /// Registers a new compound.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Register a compound")]
    [ProducesResponseType<CompoundResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompoundResponse>> Create(
        CreateCompoundRequest request,
        CancellationToken cancellationToken)
    {
        var duplicateExists = await dbContext.Compounds.AnyAsync(
            compound =>
                request.InchiKey != null &&
                compound.InchiKey == request.InchiKey,
            cancellationToken);

        if (duplicateExists)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Compound already exists",
                detail: $"A compound with InChIKey '{request.InchiKey}' already exists.");
        }

        var timestamp = DateTimeOffset.UtcNow;

        var compound = new Compound
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            CanonicalSmiles = request.CanonicalSmiles.Trim(),
            InchiKey = request.InchiKey?.Trim().ToUpperInvariant(),
            MolecularWeight = request.MolecularWeight,
            MolecularFormula = request.MolecularFormula?.Trim(),
            Source = request.Source?.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        dbContext.Compounds.Add(compound);
        await dbContext.SaveChangesAsync(cancellationToken);

        var integrationEvent = new CompoundRegistered
        {
            CompoundId = compound.Id,
            Name = compound.Name,
            CanonicalSmiles = compound.CanonicalSmiles,
            InchiKey = compound.InchiKey,
            MolecularWeight = compound.MolecularWeight,
            MolecularFormula = compound.MolecularFormula,
            Status = compound.Status
        };

        await eventPublisher.PublishAsync(
            integrationEvent,
            HttpContext.TraceIdentifier,
            cancellationToken);

        logger.LogInformation(
            "Registered compound {CompoundId} with name {CompoundName}",
            compound.Id,
            compound.Name);

        return CreatedAtAction(
            nameof(GetById),
            new { id = compound.Id },
            compound.ToResponse());
    }

    /// <summary>
    /// Replaces the editable information for an existing compound.
    /// </summary>
    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a compound")]
    [ProducesResponseType<CompoundResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompoundResponse>> Update(
        Guid id,
        UpdateCompoundRequest request,
        CancellationToken cancellationToken)
    {
        var compound = await dbContext.Compounds.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);

        if (compound is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Compound not found",
                detail: $"No compound with ID '{id}' was found.");
        }

        var duplicateExists = await dbContext.Compounds.AnyAsync(
            item =>
                item.Id != id &&
                request.InchiKey != null &&
                item.InchiKey == request.InchiKey,
            cancellationToken);

        if (duplicateExists)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Compound already exists",
                detail: $"Another compound with InChIKey '{request.InchiKey}' already exists.");
        }

        var previousStatus = compound.Status;

        compound.Name = request.Name.Trim();
        compound.CanonicalSmiles = request.CanonicalSmiles.Trim();
        compound.InchiKey = request.InchiKey?.Trim().ToUpperInvariant();
        compound.MolecularWeight = request.MolecularWeight;
        compound.MolecularFormula = request.MolecularFormula?.Trim();
        compound.Source = request.Source?.Trim();
        compound.Description = request.Description?.Trim();
        compound.Status = request.Status;
        compound.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
    new CompoundUpdated
    {
        CompoundId = compound.Id,
        Name = compound.Name,
        CanonicalSmiles = compound.CanonicalSmiles,
        InchiKey = compound.InchiKey,
        MolecularWeight = compound.MolecularWeight,
        MolecularFormula = compound.MolecularFormula,
        Status = compound.Status
    },
    HttpContext.TraceIdentifier,
    cancellationToken);

        if (previousStatus != compound.Status)
        {
            await eventPublisher.PublishAsync(
                new CompoundStatusChanged
                {
                    CompoundId = compound.Id,
                    PreviousStatus = previousStatus,
                    CurrentStatus = compound.Status
                },
                HttpContext.TraceIdentifier,
                cancellationToken);
        }

        return Ok(compound.ToResponse());
    }

    /// <summary>
    /// Removes a compound from the prototype catalog.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a compound")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var compound = await dbContext.Compounds.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);

        if (compound is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Compound not found",
                detail: $"No compound with ID '{id}' was found.");
        }

        var deletedEvent = new CompoundDeleted
        {
            CompoundId = compound.Id,
            Name = compound.Name
        };

        dbContext.Compounds.Remove(compound);
        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            deletedEvent,
            HttpContext.TraceIdentifier,
            cancellationToken);

        logger.LogInformation(
            "Deleted compound {CompoundId}",
            compound.Id);

        return NoContent();
    }
}