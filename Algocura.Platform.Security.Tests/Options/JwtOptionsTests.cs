using Algocura.Platform.Security.Options;
using FluentAssertions;

namespace Algocura.Platform.Security.Tests.Options;

public sealed class JwtOptionsTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var options = new JwtOptions();

        JwtOptions.SectionName.Should().Be("Jwt");
        options.Issuer.Should().Be("Algocura.Identity.Api");
        options.Audience.Should().Be("Algocura.Platform");
        options.SigningKey.Should().BeEmpty();
        options.AccessTokenMinutes.Should().Be(60);
        options.ClockSkewMinutes.Should().Be(5);
    }
}
