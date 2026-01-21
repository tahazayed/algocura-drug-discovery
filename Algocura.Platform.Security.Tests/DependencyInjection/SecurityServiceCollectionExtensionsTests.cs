using Algocura.Platform.Security.Authorization;
using Algocura.Platform.Security.Context;
using Algocura.Platform.Security.DependencyInjection;
using Algocura.Platform.Security.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Algocura.Platform.Security.Tests.DependencyInjection;

public sealed class SecurityServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAlgocuraPlatformSecurity_RegistersCoreSecurityServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAlgocuraPlatformSecurity(CreateConfiguration());

        services.Should().Contain(d =>
            d.ServiceType == typeof(IHttpContextAccessor));

        services.Should().Contain(d =>
            d.ServiceType == typeof(ICurrentUserContext) &&
            d.ImplementationType == typeof(HttpCurrentUserContext) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddAlgocuraPlatformSecurity_BindsJwtOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAlgocuraPlatformSecurity(CreateConfiguration());

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

        options.Issuer.Should().Be("Algocura.Identity.Api");
        options.Audience.Should().Be("Algocura.Platform");
        options.SigningKey.Should().HaveLength(64);
        options.AccessTokenMinutes.Should().Be(45);
        options.ClockSkewMinutes.Should().Be(2);
    }

    [Fact]
    public void AddAlgocuraPlatformSecurity_ConfiguresJwtBearerAsDefaultScheme()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAlgocuraPlatformSecurity(CreateConfiguration());

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        options.DefaultAuthenticateScheme.Should().Be(JwtBearerDefaults.AuthenticationScheme);
        options.DefaultChallengeScheme.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Theory]
    [InlineData(PlatformPolicies.AuthenticatedUser)]
    [InlineData(PlatformPolicies.AdministratorOnly)]
    [InlineData(PlatformPolicies.ScientistOrAdministrator)]
    [InlineData(PlatformPolicies.ReviewerOrAdministrator)]
    [InlineData(PlatformPolicies.ServiceOrAdministrator)]
    public async Task AddAlgocuraPlatformSecurity_RegistersBuiltInPolicies(string policyName)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAlgocuraPlatformSecurity(CreateConfiguration());

        using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(policyName);

        policy.Should().NotBeNull();
    }

    [Fact]
    public async Task AdministratorOnlyPolicy_RequiresAdministratorRole()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAlgocuraPlatformSecurity(CreateConfiguration());

        using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var policy = await policyProvider.GetPolicyAsync(PlatformPolicies.AdministratorOnly);

        policy.Should().NotBeNull();

        policy!.Requirements
            .OfType<RolesAuthorizationRequirement>()
            .Should()
            .ContainSingle()
            .Which.AllowedRoles
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(PlatformRoles.Administrator);
    }

    private static IConfiguration CreateConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "Algocura.Identity.Api",
            ["Jwt:Audience"] = "Algocura.Platform",
            ["Jwt:SigningKey"] = new string('A', 64),
            ["Jwt:AccessTokenMinutes"] = "45",
            ["Jwt:ClockSkewMinutes"] = "2"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
