using Algocura.Platform.Auditing.Identity;
using Algocura.Platform.Auditing.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Algocura.Platform.Auditing.Tests.Identity;

public sealed class CurrentUserContextTests
{
    [Fact]
    public void AuthenticatedUser_ReadsClaims()
    {
        var userId =
            Guid.NewGuid();

        var identity =
            new ClaimsIdentity(
                [
                    new Claim("sub", userId.ToString()),
                    new Claim("name", "Test User"),
                    new Claim("email", "test@example.com")
                ],
                "Test");

        var context =
            new DefaultHttpContext
            {
                User =
                    new ClaimsPrincipal(identity)
            };

        context.Request.Headers["X-Correlation-ID"] =
            "corr-123";

        var sut =
            new CurrentUserContext(
                new HttpContextAccessor
                {
                    HttpContext = context
                });

        sut.UserId.Should().Be(userId);
        sut.DisplayName.Should().Be("Test User");
        sut.Email.Should().Be("test@example.com");
        sut.ActorType.Should().Be(AuditActorType.User);
        sut.CorrelationId.Should().Be("corr-123");
        sut.IsAuthenticated.Should().BeTrue();
    }
}
