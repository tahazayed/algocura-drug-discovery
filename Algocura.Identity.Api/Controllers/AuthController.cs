using Algocura.Identity.Api.Contracts;
using Algocura.Identity.Api.Data;
using Algocura.Identity.Api.Domain;
using Algocura.Identity.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Algocura.Identity.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService tokenService)
    : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await userManager.FindByEmailAsync(
                request.Email);

        if (existing is not null)
        {
            return Conflict(
                new
                {
                    message =
                        "A user with this email already exists."
                });
        }

        var now =
            DateTimeOffset.UtcNow;

        var user =
            new ApplicationUser
            {
                Id =
                    Guid.NewGuid(),

                UserName =
                    request.Email.Trim(),

                Email =
                    request.Email.Trim(),

                DisplayName =
                    request.DisplayName?.Trim(),

                IsActive =
                    true,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedAtUtc =
                    now,

                UpdatedAtUtc =
                    now
            };

        var result =
            await userManager.CreateAsync(
                user,
                request.Password);

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

        await userManager.AddToRoleAsync(
            user,
            "Scientist");

        var roles =
            await userManager.GetRolesAsync(user);

        return Created(
            $"/api/v1/users/{user.Id}",
            new UserResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                user.IsActive,
                user.LastLoginAtUtc,
                roles.ToArray()));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user =
            await userManager.FindByEmailAsync(
                request.Email);

        if (user is null ||
            !user.IsActive)
        {
            return Unauthorized();
        }

        var result =
            await signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure:
                    true);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        user.LastLoginAtUtc =
            DateTimeOffset.UtcNow;

        user.UpdatedByUserId =
            user.Id;

        user.UpdatedAtUtc =
            DateTimeOffset.UtcNow;

        await userManager.UpdateAsync(user);

        var token =
            await tokenService.CreateAsync(
                user,
                cancellationToken);

        var roles =
            await userManager.GetRolesAsync(user);

        return Ok(
            new TokenResponse(
                token.Token,
                token.ExpiresAtUtc,
                new UserResponse(
                    user.Id,
                    user.Email ?? string.Empty,
                    user.DisplayName,
                    user.IsActive,
                    user.LastLoginAtUtc,
                    roles.ToArray())));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me(
        CancellationToken cancellationToken = default)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var userId))
        {
            return Unauthorized();
        }

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

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
}
