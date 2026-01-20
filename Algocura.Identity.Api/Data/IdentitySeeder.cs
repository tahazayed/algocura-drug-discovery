using Algocura.Identity.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace Algocura.Identity.Api.Data;

public static class IdentitySeeder
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(
            2026,
            1,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        var roleManager =
            services.GetRequiredService<
                RoleManager<ApplicationRole>>();

        await EnsureRoleAsync(
            roleManager,
            "Administrator",
            "Platform administrator.",
            cancellationToken);

        await EnsureRoleAsync(
            roleManager,
            "Scientist",
            "Scientific platform user.",
            cancellationToken);

        await EnsureRoleAsync(
            roleManager,
            "Reviewer",
            "Scientific reviewer and approver.",
            cancellationToken);

        await EnsureRoleAsync(
            roleManager,
            "Service",
            "Machine or service identity.",
            cancellationToken);

        await EnsureSystemUserAsync(
            userManager,
            cancellationToken);

        var administratorPassword =
            configuration[
                "Seed:AdministratorPassword"];

        if (!string.IsNullOrWhiteSpace(
                administratorPassword))
        {
            await EnsureAdministratorAsync(
                userManager,
                administratorPassword,
                cancellationToken);
        }
    }

    private static async Task EnsureRoleAsync(
        RoleManager<ApplicationRole> roleManager,
        string name,
        string description,
        CancellationToken cancellationToken)
    {
        var existing =
            await roleManager.FindByNameAsync(
                name);

        if (existing is not null)
        {
            return;
        }

        var role =
            new ApplicationRole
            {
                Id =
                    Guid.NewGuid(),

                Name =
                    name,

                NormalizedName =
                    name.ToUpperInvariant(),

                Description =
                    description,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp
            };

        var result =
            await roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(x =>
                        $"{x.Code}: {x.Description}")));
        }
    }

    private static async Task EnsureSystemUserAsync(
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var existing =
            await userManager.FindByIdAsync(
                SystemUsers.SystemUserId.ToString());

        if (existing is not null)
        {
            return;
        }

        var user =
            new ApplicationUser
            {
                Id =
                    SystemUsers.SystemUserId,

                UserName =
                    "system@algocura.local",

                Email =
                    "system@algocura.local",

                EmailConfirmed =
                    true,

                DisplayName =
                    "Algocura System",

                IsActive =
                    true,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp
            };

        var result =
            await userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(x =>
                        $"{x.Code}: {x.Description}")));
        }

        await userManager.AddToRoleAsync(
            user,
            "Service");
    }

    private static async Task EnsureAdministratorAsync(
        UserManager<ApplicationUser> userManager,
        string password,
        CancellationToken cancellationToken)
    {
        var existing =
            await userManager.FindByIdAsync(
                SystemUsers.AdministratorUserId
                    .ToString());

        if (existing is not null)
        {
            return;
        }

        var user =
            new ApplicationUser
            {
                Id =
                    SystemUsers.AdministratorUserId,

                UserName =
                    "admin@algocura.local",

                Email =
                    "admin@algocura.local",

                EmailConfirmed =
                    true,

                DisplayName =
                    "Algocura Administrator",

                IsActive =
                    true,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp
            };

        var result =
            await userManager.CreateAsync(
                user,
                password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(x =>
                        $"{x.Code}: {x.Description}")));
        }

        await userManager.AddToRoleAsync(
            user,
            "Administrator");
    }
}
