using Algocura.Platform.Security.Claims;
using FluentAssertions;

namespace Algocura.Platform.Security.Tests.Claims;

public sealed class PlatformClaimTypesTests
{
    [Fact]
    public void ClaimTypeNames_AreStable()
    {
        PlatformClaimTypes.Subject.Should().Be("sub");
        PlatformClaimTypes.Email.Should().Be("email");
        PlatformClaimTypes.CorrelationId.Should().Be("correlation_id");
        PlatformClaimTypes.WorkflowId.Should().Be("workflow_id");
    }
}
