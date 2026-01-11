using Algocura.AIDiscovery.Api.Contracts;
using Algocura.AIDiscovery.Api.Data;
using Algocura.AIDiscovery.Api.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AIDiscovery.Api.Controllers;

[ApiController]
[Route("api/v1/models")]
public sealed class ModelsController(AIDiscoveryDbContext db)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<ModelDefinitionResponse>>> GetAll(
            CancellationToken cancellationToken)
    {
        var models = await db.ModelDefinitions
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);

        return Ok(models.Select(x => x.ToResponse()).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ModelDefinitionResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var model = await db.ModelDefinitions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return model is null
            ? NotFound()
            : Ok(model.ToResponse());
    }
}
