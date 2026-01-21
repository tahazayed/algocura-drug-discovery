# Algocura.Platform.Security.Tests

Unit tests for `Algocura.Platform.Security`.

## Coverage

```text
PlatformRoles
PlatformPolicies
PlatformClaimTypes
JwtOptions defaults
ClaimsPrincipal extensions
HttpCurrentUserContext
Dependency injection
JWT configuration binding
Default JWT bearer scheme
Built-in authorization policies
Role policy requirements
```

## Run

```powershell
dotnet test .\Algocura.Platform.Security.Tests\Algocura.Platform.Security.Tests.csproj
```

## Add to the solution

```powershell
dotnet sln add .\Algocura.Platform.Security.Tests\Algocura.Platform.Security.Tests.csproj
```

The tests use xUnit and FluentAssertions and do not require the Identity database
or a live identity provider.
