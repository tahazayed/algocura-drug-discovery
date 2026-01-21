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

    // -------------------------------------------------------------------------
    // Seed user IDs
    //
    // These IDs are deterministic so repeated seeding remains idempotent.
    // -------------------------------------------------------------------------

    private static readonly Guid ScientistUserId =
        Guid.Parse(
            "00000000-0000-0000-0001-000000000001");

    private static readonly Guid ReviewerUserId =
        Guid.Parse(
            "00000000-0000-0000-0002-000000000001");

    private static readonly Guid ServiceUserId =
        Guid.Parse(
            "00000000-0000-0000-0000-000000000001");

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

        // ---------------------------------------------------------------------
        // Roles
        // ---------------------------------------------------------------------

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

        // ---------------------------------------------------------------------
        // System user
        //
        // The system user is assigned to the Service role. Because the Service
        // role has the fixed ID above, the AspNetUserRoles record will contain:
        //
        // RoleId = 00000000-0000-0000-0000-000000000001
        // ---------------------------------------------------------------------

        await EnsureSystemUserAsync(
            userManager,
            cancellationToken);

        // ---------------------------------------------------------------------
        // Administrator
        // ---------------------------------------------------------------------

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

        // ---------------------------------------------------------------------
        // Scientist
        // ---------------------------------------------------------------------

        var scientistPassword =
            configuration[
                "Seed:ScientistPassword"];

        if (!string.IsNullOrWhiteSpace(
                scientistPassword))
        {
            await EnsureSeedUserAsync(
                userManager,
                ScientistUserId,
                "scientist@algocura.local",
                "Algocura Scientist",
                scientistPassword,
                "Scientist",
                cancellationToken);
        }

        // ---------------------------------------------------------------------
        // Reviewer
        // ---------------------------------------------------------------------

        var reviewerPassword =
            configuration[
                "Seed:ReviewerPassword"];

        if (!string.IsNullOrWhiteSpace(
                reviewerPassword))
        {
            await EnsureSeedUserAsync(
                userManager,
                ReviewerUserId,
                "reviewer@algocura.local",
                "Algocura Reviewer",
                reviewerPassword,
                "Reviewer",
                cancellationToken);
        }

        // ---------------------------------------------------------------------
        // Service test user
        //
        // This is a normal credential-based Service-role user for testing.
        // The special system user above remains passwordless.
        // ---------------------------------------------------------------------

        var servicePassword =
            configuration[
                "Seed:ServicePassword"];

        if (!string.IsNullOrWhiteSpace(
                servicePassword))
        {
            await EnsureSeedUserAsync(
                userManager,
                ServiceUserId,
                "service@algocura.local",
                "Algocura Service",
                servicePassword,
                "Service",
                cancellationToken);
        }
    }

    private static async Task EnsureRoleAsync(
        RoleManager<ApplicationRole> roleManager,
        string name,
        string description,
        CancellationToken cancellationToken,
        Guid? roleId = null)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existing =
            await roleManager.FindByNameAsync(
                name);

        if (existing is not null)
        {
            // The Service role must always use the required deterministic ID.
            if (roleId.HasValue &&
                existing.Id != roleId.Value)
            {
                throw new InvalidOperationException(
                    $"Role '{name}' already exists with ID " +
                    $"'{existing.Id}', but the required ID is " +
                    $"'{roleId.Value}'.");
            }

            return;
        }

        var role =
            new ApplicationRole
            {
                Id =
                    roleId ??
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
            await roleManager.CreateAsync(
                role);

        EnsureSucceeded(
            result,
            $"create role '{name}'");
    }

    private static async Task EnsureSystemUserAsync(
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await userManager.FindByIdAsync(
                SystemUsers.SystemUserId.ToString());

        if (user is null)
        {
            user =
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

            var createResult =
                await userManager.CreateAsync(
                    user);

            EnsureSucceeded(
                createResult,
                "create system user");
        }

        await EnsureUserRoleAsync(
            userManager,
            user,
            "Service");
    }

    private static async Task EnsureAdministratorAsync(
        UserManager<ApplicationUser> userManager,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await userManager.FindByIdAsync(
                SystemUsers.AdministratorUserId
                    .ToString());

        if (user is null)
        {
            user =
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

            var createResult =
                await userManager.CreateAsync(
                    user,
                    password);

            EnsureSucceeded(
                createResult,
                "create administrator user");
        }

        await EnsureUserRoleAsync(
            userManager,
            user,
            "Administrator");
    }

    private static async Task EnsureSeedUserAsync(
        UserManager<ApplicationUser> userManager,
        Guid userId,
        string email,
        string displayName,
        string password,
        string roleName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        if (user is null)
        {
            user =
                await userManager.FindByEmailAsync(
                    email);
        }

        if (user is null)
        {
            user =
                new ApplicationUser
                {
                    Id =
                        userId,

                    UserName =
                        email,

                    Email =
                        email,

                    EmailConfirmed =
                        true,

                    DisplayName =
                        displayName,

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

            var createResult =
                await userManager.CreateAsync(
                    user,
                    password);

            EnsureSucceeded(
                createResult,
                $"create seed user '{email}'");
        }

        await EnsureUserRoleAsync(
            userManager,
            user,
            roleName);
    }

    private static async Task EnsureUserRoleAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string roleName)
    {
        if (await userManager.IsInRoleAsync(
                user,
                roleName))
        {
            return;
        }

        var result =
            await userManager.AddToRoleAsync(
                user,
                roleName);

        EnsureSucceeded(
            result,
            $"assign role '{roleName}' to user '{user.UserName}'");
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Failed to {operation}: " +
            string.Join(
                "; ",
                result.Errors.Select(
                    error =>
                        $"{error.Code}: {error.Description}")));
    }
}