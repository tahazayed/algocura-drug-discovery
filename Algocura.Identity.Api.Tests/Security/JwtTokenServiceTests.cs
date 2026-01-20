using Algocura.Identity.Api.Domain;
using Algocura.Identity.Api.Security;
using Algocura.Identity.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Algocura.Identity.Api.Tests.Security;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public async Task CreateAsync_IncludesUserAndRoleClaims()
    {
        await using var services =
            TestIdentityFactory.Create();

        var roleManager =
            services.GetRequiredService<
                RoleManager<ApplicationRole>>();

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        await roleManager.CreateAsync(
            new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = "Scientist",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            });

        var user =
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "test@algocura.local",
                Email = "test@algocura.local",
                DisplayName = "Test Scientist",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

        var createResult =
            await userManager.CreateAsync(
                user,
                "Example!Pass123");

        createResult.Succeeded.Should().BeTrue();

        await userManager.AddToRoleAsync(
            user,
            "Scientist");

        var options =
            Options.Create(
                new JwtOptions
                {
                    Issuer =
                        "Algocura.Identity.Api",

                    Audience =
                        "Algocura.Platform",

                    SigningKey =
                        "0123456789012345678901234567890123456789",

                    AccessTokenMinutes =
                        60
                });

        var service =
            new JwtTokenService(
                userManager,
                options);

        var token =
            await service.CreateAsync(user);

        token.Token.Should()
            .NotBeNullOrWhiteSpace();

        var jwt =
            new JwtSecurityTokenHandler()
                .ReadJwtToken(
                    token.Token);

        jwt.Claims.Should()
            .Contain(
                x =>
                    x.Type ==
                        ClaimTypes.Role &&
                    x.Value ==
                        "Scientist");
    }
}
