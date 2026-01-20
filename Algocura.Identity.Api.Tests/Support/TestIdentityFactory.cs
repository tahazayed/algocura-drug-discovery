using Algocura.Identity.Api.Data;
using Algocura.Identity.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Algocura.Identity.Api.Tests.Support;

internal static class TestIdentityFactory
{
    public static ServiceProvider Create()
    {
        var services =
            new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<IdentityDbContext>(
            options =>
                options.UseInMemoryDatabase(
                    $"IdentityTests-{Guid.NewGuid()}"));

        services
            .AddIdentityCore<ApplicationUser>(
                options =>
                {
                    options.User.RequireUniqueEmail =
                        true;

                    options.Password.RequiredLength =
                        12;

                    options.Password.RequireDigit =
                        true;

                    options.Password.RequireUppercase =
                        true;

                    options.Password.RequireLowercase =
                        true;

                    options.Password.RequireNonAlphanumeric =
                        true;
                })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityDbContext>();

        return services.BuildServiceProvider();
    }
}
