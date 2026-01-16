using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Options;
using Microsoft.Extensions.Options;

namespace Algocura.Platform.Auditing.Sanitization;

public sealed class DefaultAuditValueSanitizer(
    IOptions<AuditingOptions> options)
    : IAuditValueSanitizer
{
    private readonly AuditingOptions _options = options.Value;

    public IReadOnlyDictionary<string, object?> Sanitize(
        string entityType,
        IReadOnlyDictionary<string, object?> values)
    {
        var result =
            new Dictionary<string, object?>(
                StringComparer.Ordinal);

        foreach (var pair in values)
        {
            if (_options.ExcludedPropertyNames.Contains(pair.Key))
            {
                continue;
            }

            result[pair.Key] =
                _options.MaskedPropertyNames.Contains(pair.Key)
                    ? Mask(pair.Value)
                    : pair.Value;
        }

        return result;
    }

    private static object? Mask(object? value)
    {
        if (value is null)
        {
            return null;
        }

        var text = value.ToString();

        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        if (text.Contains('@'))
        {
            var parts = text.Split('@', 2);

            return parts[0].Length <= 2
                ? $"***@{parts[1]}"
                : $"{parts[0][..2]}***@{parts[1]}";
        }

        return text.Length <= 4
            ? "****"
            : $"{text[..2]}***{text[^2..]}";
    }
}
