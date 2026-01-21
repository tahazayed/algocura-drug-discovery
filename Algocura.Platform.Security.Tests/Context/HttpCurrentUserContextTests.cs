using Algocura.Platform.Security.Authorization;
using Algocura.Platform.Security.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Algocura.Platform.Security.Tests.Context;

public sealed class HttpCurrentUserContextTests
{
    [Fact]
    public void AuthenticatedUser_ExposesCurrentUserMetadata()
    {
        var userId = Guid.NewGuid();

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, "Algocura Scientist"),
                    new Claim(ClaimTypes.Email, "scientist@algocura.test"),
                    new Claim(ClaimTypes.Role, PlatformRoles.Scientist)
                ],
                "Test"))
        };

        var accessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };

        var sut = new HttpCurrentUserContext(accessor);

        sut.IsAuthenticated.Should().BeTrue();
        sut.UserId.Should().Be(userId);
        sut.DisplayName.Should().Be("Algocura Scientist");
        sut.Email.Should().Be("scientist@algocura.test");
        sut.Roles.Should().ContainSingle().Which.Should().Be(PlatformRoles.Scientist);
        sut.IsInRole(PlatformRoles.Scientist).Should().BeTrue();
        sut.IsInRole(PlatformRoles.Administrator).Should().BeFalse();
    }

    [Fact]
    public void MissingHttpContext_ReturnsAnonymousValues()
    {
        var sut = new HttpCurrentUserContext(new HttpContextAccessor());

        sut.IsAuthenticated.Should().BeFalse();
        sut.UserId.Should().BeNull();
        sut.DisplayName.Should().BeNull();
        sut.Email.Should().BeNull();
        sut.Roles.Should().BeEmpty();
        sut.IsInRole(PlatformRoles.Administrator).Should().BeFalse();
    }
}
