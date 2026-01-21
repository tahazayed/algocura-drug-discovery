namespace Algocura.Platform.Security.Authorization;

public static class PlatformPolicies
{
    public const string AuthenticatedUser =
        "AuthenticatedUser";

    public const string AdministratorOnly =
        "AdministratorOnly";

    public const string ScientistOrAdministrator =
        "ScientistOrAdministrator";

    public const string ReviewerOrAdministrator =
        "ReviewerOrAdministrator";

    public const string ServiceOrAdministrator =
        "ServiceOrAdministrator";
}
