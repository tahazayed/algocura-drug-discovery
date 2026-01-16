namespace Algocura.Platform.Auditing.Abstractions;

/// <summary>
/// Removes or masks sensitive values before they are serialized into audit events.
/// </summary>
public interface IAuditValueSanitizer
{
    IReadOnlyDictionary<string, object?> Sanitize(
        string entityType,
        IReadOnlyDictionary<string, object?> values);
}

