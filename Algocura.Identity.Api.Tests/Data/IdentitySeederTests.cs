using Algocura.Identity.Api.Data;
using Algocura.Identity.Api.Domain;
using Algocura.Identity.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Algocura.Identity.Api.Tests.Data;

public sealed class IdentitySeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesRolesAndSystemUser()
    {
        await using var services =
            TestIdentityFactory.Create();

        var configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>())
                .Build();

        await IdentitySeeder.SeedAsync(
            services,
            configuration);

        var roleManager =
            services.GetRequiredService<
                RoleManager<ApplicationRole>>();

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        (await roleManager.RoleExistsAsync(
                "Administrator"))
            .Should()
            .BeTrue();

        (await roleManager.RoleExistsAsync(
                "Scientist"))
            .Should()
            .BeTrue();

        var systemUser =
            await userManager.FindByIdAsync(
                SystemUsers.SystemUserId.ToString());

        systemUser.Should().NotBeNull();

        var roles =
            await userManager.GetRolesAsync(
                systemUser!);

        roles.Should().Contain("Service");
    }
}
