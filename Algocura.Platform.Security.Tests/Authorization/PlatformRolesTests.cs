using Algocura.Platform.Security.Authorization;
using FluentAssertions;

namespace Algocura.Platform.Security.Tests.Authorization;

public sealed class PlatformRolesTests
{
    [Fact]
    public void All_ContainsExpectedPlatformRoles()
    {
        PlatformRoles.All.Should().BeEquivalentTo(
        [
            PlatformRoles.Administrator,
            PlatformRoles.Scientist,
            PlatformRoles.Reviewer,
            PlatformRoles.Service
        ]);
    }

    [Fact]
    public void RoleNames_AreStable()
    {
        PlatformRoles.Administrator.Should().Be("Administrator");
        PlatformRoles.Scientist.Should().Be("Scientist");
        PlatformRoles.Reviewer.Should().Be("Reviewer");
        PlatformRoles.Service.Should().Be("Service");
    }
}
