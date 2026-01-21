namespace Algocura.Platform.Security.Authorization;

public static class PlatformRoles
{
    public const string Administrator =
        "Administrator";

    public const string Scientist =
        "Scientist";

    public const string Reviewer =
        "Reviewer";

    public const string Service =
        "Service";

    public static IReadOnlyCollection<string> All { get; } =
    [
        Administrator,
        Scientist,
        Reviewer,
        Service
    ];
}
