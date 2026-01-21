using Algocura.Platform.Security.Claims;
using Algocura.Platform.Security.Extensions;
using FluentAssertions;
using System.Security.Claims;

namespace Algocura.Platform.Security.Tests.Extensions;

public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_UsesNameIdentifierClaim()
    {
        var userId = Guid.NewGuid();
        var principal = CreatePrincipal(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ]);

        principal.GetUserId().Should().Be(userId);
    }

    [Fact]
    public void GetUserId_FallsBackToSubjectClaim()
    {
        var userId = Guid.NewGuid();
        var principal = CreatePrincipal(
        [
            new Claim(PlatformClaimTypes.Subject, userId.ToString())
        ]);

        principal.GetUserId().Should().Be(userId);
    }

    [Fact]
    public void GetUserId_WithInvalidGuid_ReturnsNull()
    {
        var principal = CreatePrincipal(
        [
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        ]);

        principal.GetUserId().Should().BeNull();
    }

    [Fact]
    public void GetDisplayName_ReturnsNameClaim()
    {
        var principal = CreatePrincipal(
        [
            new Claim(ClaimTypes.Name, "Test Scientist")
        ]);

        principal.GetDisplayName().Should().Be("Test Scientist");
    }

    [Fact]
    public void GetEmail_UsesEmailClaim()
    {
        var principal = CreatePrincipal(
        [
            new Claim(ClaimTypes.Email, "scientist@algocura.test")
        ]);

        principal.GetEmail().Should().Be("scientist@algocura.test");
    }

    [Fact]
    public void GetRoles_ReturnsDistinctRolesIgnoringCase()
    {
        var principal = CreatePrincipal(
        [
            new Claim(ClaimTypes.Role, "Scientist"),
            new Claim(ClaimTypes.Role, "scientist"),
            new Claim(ClaimTypes.Role, "Reviewer")
        ]);

        principal.GetRoles().Should().BeEquivalentTo(
        [
            "Scientist",
            "Reviewer"
        ]);
    }

    private static ClaimsPrincipal CreatePrincipal(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}
