using Algocura.Platform.Auditing.Options;
using Algocura.Platform.Auditing.Sanitization;
using FluentAssertions;

namespace Algocura.Platform.Auditing.Tests.Sanitization;

public sealed class DefaultAuditValueSanitizerTests
{
    [Fact]
    public void Sanitize_ExcludesSecretsAndMasksEmail()
    {
        var sanitizer =
            new DefaultAuditValueSanitizer(
                Microsoft.Extensions.Options.Options.Create(
                    new AuditingOptions()));

        var result =
            sanitizer.Sanitize(
                "TestEntity",
                new Dictionary<string, object?>
                {
                    ["Name"] = "Test",
                    ["Password"] = "Secret",
                    ["Email"] = "test@example.com"
                });

        result.Should()
            .ContainKey("Name");

        result.Should()
            .NotContainKey("Password");

        result["Email"].Should()
            .Be("te***@example.com");
    }
}