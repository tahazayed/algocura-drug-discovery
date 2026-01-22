using Algocura.AssayManagement.Api.Contracts;
using Algocura.AssayManagement.Api.Data;
using Algocura.AssayManagement.Api.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AssayManagement.Api.Controllers;

[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/completed-screenings")]
public sealed class CompletedScreeningsController(
    AssayManagementDbContext dbContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<CompletedScreeningResponse>>> GetAll(
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
    {
        var screenings = await dbContext.Screenings
            .AsNoTracking()
            .Include(x => x.Results)
            .OrderByDescending(x => x.CompletedAtUtc)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken);

        return Ok(
            screenings
                .Select(x => x.ToResponse())
                .ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompletedScreeningResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var screening = await dbContext.Screenings
            .AsNoTracking()
            .Include(x => x.Results)
            .SingleOrDefaultAsync(
                x => x.ScreeningId == id,
                cancellationToken);

        return screening is null
            ? NotFound()
            : Ok(screening.ToResponse());
    }
}

