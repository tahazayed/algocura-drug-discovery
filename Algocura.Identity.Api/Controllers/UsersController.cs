using Algocura.Identity.Api.Contracts;
using Algocura.Identity.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Identity.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/v1/users")]
public sealed class UsersController(
    UserManager<ApplicationUser> userManager)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyCollection<UserResponse>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var users =
            await userManager.Users
                .AsNoTracking()
                .OrderBy(x => x.Email)
                .ToListAsync(cancellationToken);

        var responses =
            new List<UserResponse>();

        foreach (var user in users)
        {
            var roles =
                await userManager.GetRolesAsync(user);

            responses.Add(
                new UserResponse(
                    user.Id,
                    user.Email ?? string.Empty,
                    user.DisplayName,
                    user.IsActive,
                    user.LastLoginAtUtc,
                    roles.ToArray()));
        }

        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user =
            await userManager.FindByIdAsync(
                id.ToString());

        if (user is null)
        {
            return NotFound();
        }

        var roles =
            await userManager.GetRolesAsync(user);

        return Ok(
            new UserResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                user.IsActive,
                user.LastLoginAtUtc,
                roles.ToArray()));
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(
        Guid id,
        AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var user =
            await userManager.FindByIdAsync(
                id.ToString());

        if (user is null)
        {
            return NotFound();
        }

        var result =
            await userManager.AddToRoleAsync(
                user,
                request.RoleName);

        if (!result.Succeeded)
        {
            return ValidationProblem(
                new ValidationProblemDetails(
                    result.Errors
                        .GroupBy(x => x.Code)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Select(
                                    error =>
                                        error.Description)
                                .ToArray())));
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/disable")]
    public async Task<IActionResult> Disable(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user =
            await userManager.FindByIdAsync(
                id.ToString());

        if (user is null)
        {
            return NotFound();
        }

        user.IsActive =
            false;

        user.UpdatedByUserId =
            id;

        user.UpdatedAtUtc =
            DateTimeOffset.UtcNow;

        var result =
            await userManager.UpdateAsync(user);

        return result.Succeeded
            ? NoContent()
            : Problem(
                "Unable to disable user.");
    }
}
