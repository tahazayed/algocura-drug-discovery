using Algocura.Platform.Security.Authorization;
using FluentAssertions;

namespace Algocura.Platform.Security.Tests.Authorization;

public sealed class PlatformPoliciesTests
{
    [Fact]
    public void PolicyNames_AreStable()
    {
        PlatformPolicies.AuthenticatedUser.Should().Be("AuthenticatedUser");
        PlatformPolicies.AdministratorOnly.Should().Be("AdministratorOnly");
        PlatformPolicies.ScientistOrAdministrator.Should().Be("ScientistOrAdministrator");
        PlatformPolicies.ReviewerOrAdministrator.Should().Be("ReviewerOrAdministrator");
        PlatformPolicies.ServiceOrAdministrator.Should().Be("ServiceOrAdministrator");
    }
}
