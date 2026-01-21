# Algocura.Platform.Security

Shared authentication and authorization infrastructure for Algocura APIs.

The project centralizes JWT bearer validation, platform role names,
authorization policy names, and access to the authenticated user.

It is designed to be used by the Identity API and all protected downstream
Algocura APIs.

---

## Responsibilities

`Algocura.Platform.Security` provides:

```text
JWT configuration
JWT bearer authentication
Issuer validation
Audience validation
Signing-key validation
Token lifetime validation
Role claim mapping
Shared platform role constants
Shared authorization policies
Current authenticated-user context
ClaimsPrincipal helper methods
```

The project does not own:

```text
User persistence
Role persistence
Password hashing
Login validation
Token issuance
Identity database
```

Those responsibilities remain in `Algocura.Identity.Api`.

---

## Project Structure

```text
Algocura.Platform.Security
│
├── Authorization
│   ├── PlatformPolicies.cs
│   └── PlatformRoles.cs
│
├── Claims
│   └── PlatformClaimTypes.cs
│
├── Context
│   ├── ICurrentUserContext.cs
│   └── HttpCurrentUserContext.cs
│
├── DependencyInjection
│   └── SecurityServiceCollectionExtensions.cs
│
├── Extensions
│   └── ClaimsPrincipalExtensions.cs
│
├── Options
│   └── JwtOptions.cs
│
└── Algocura.Platform.Security.csproj
```

---

## Authentication Flow

```text
Algocura.Identity.Api
        │
        │ Issues signed JWT
        ▼
Client
        │
        │ Authorization: Bearer <token>
        ▼
Algocura API
        │
        ▼
Algocura.Platform.Security
        │
        ├── Validate issuer
        ├── Validate audience
        ├── Validate signing key
        ├── Validate expiration
        └── Build ClaimsPrincipal
                │
                ▼
        ASP.NET Core Authorization
                │
                ▼
        Controller / endpoint
```

---

## Roles

The shared role names are:

```text
Administrator
Scientist
Reviewer
Service
```

Use constants instead of string literals:

```csharp
PlatformRoles.Administrator
PlatformRoles.Scientist
PlatformRoles.Reviewer
PlatformRoles.Service
```

The role names intentionally match the roles seeded by
`Algocura.Identity.Api`.

---

## Built-in Policies

The library provides:

```text
AuthenticatedUser
AdministratorOnly
ScientistOrAdministrator
ReviewerOrAdministrator
ServiceOrAdministrator
```

Example:

```csharp
[Authorize(
    Policy =
        PlatformPolicies.AdministratorOnly)]
```

or:

```csharp
[Authorize(
    Policy =
        PlatformPolicies.ScientistOrAdministrator)]
```

---

## Configuration

All APIs that validate Algocura JWTs use the same logical JWT settings:

```json
"Jwt": {
  "Issuer": "Algocura.Identity.Api",
  "Audience": "Algocura.Platform",
  "SigningKey": "<development-or-secret-store-value>",
  "AccessTokenMinutes": 60,
  "ClockSkewMinutes": 1
}
```

Do not commit production signing keys to source control.

`AccessTokenMinutes` is primarily used by the Identity API when issuing
tokens. Downstream APIs can share the same configuration model even though
token issuance is not performed by this library.

---

## Register in an API

In `Program.cs`:

```csharp
using Algocura.Platform.Security.DependencyInjection;

builder.Services.AddAlgocuraPlatformSecurity(
    builder.Configuration);
```

In the middleware pipeline:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Authentication must run before authorization.

---

## Protect an Endpoint

Authenticated users:

```csharp
[Authorize(
    Policy =
        PlatformPolicies.AuthenticatedUser)]
```

Administrator only:

```csharp
[Authorize(
    Policy =
        PlatformPolicies.AdministratorOnly)]
```

Scientist or administrator:

```csharp
[Authorize(
    Policy =
        PlatformPolicies.ScientistOrAdministrator)]
```

---

## Current User Context

Application code can depend on:

```csharp
ICurrentUserContext
```

Example:

```csharp
public sealed class ExampleService(
    ICurrentUserContext currentUser)
{
    public Guid? GetCurrentUserId()
    {
        return currentUser.UserId;
    }
}
```

Available values include:

```text
IsAuthenticated
UserId
DisplayName
Email
Roles
IsInRole(...)
```

This avoids directly coupling application services to `HttpContext`.

---

## Identity API Integration

`Algocura.Identity.Api` continues to own token creation.

The Identity API should reference this project and replace its local
`JwtOptions` with:

```csharp
using Algocura.Platform.Security.Options;
```

Its `JwtTokenService` can continue creating:

```text
sub
email
ClaimTypes.NameIdentifier
ClaimTypes.Name
ClaimTypes.Role
```

claims.

The platform security library is intentionally compatible with those existing
claims by using:

```csharp
NameClaimType =
    ClaimTypes.Name;

RoleClaimType =
    ClaimTypes.Role;
```

The Identity API registers shared JWT validation with:

```csharp
builder.Services.AddAlgocuraPlatformSecurity(
    builder.Configuration);
```

and still registers its own token issuer:

```csharp
builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();
```

---

## Downstream API Integration

A downstream API such as Compound Catalog adds a project reference:

```xml
<ProjectReference
  Include="..\Algocura.Platform.Security\Algocura.Platform.Security.csproj" />
```

Then:

```csharp
builder.Services.AddAlgocuraPlatformSecurity(
    builder.Configuration);
```

and:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

A controller can then use:

```csharp
[Authorize(
    Policy =
        PlatformPolicies.ScientistOrAdministrator)]
```

No call to the Identity database is required for each request. The API
authorizes the authenticated principal represented by the validated JWT.

---

## Custom API Policies

An API can add its own domain policy after registering the shared security
infrastructure:

```csharp
builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy(
            "CompoundWrite",
            policy =>
            {
                policy.RequireAuthenticatedUser();

                policy.RequireRole(
                    PlatformRoles.Administrator,
                    PlatformRoles.Scientist);
            });
    });
```

Domain-specific authorization rules should remain with the owning service
unless they are genuinely platform-wide.

---

## Security Boundary

The intended dependency direction is:

```text
Algocura.Platform.Security
          ▲
          │
    Algocura APIs
          ▲
          │
Algocura.Identity.Api
  issues platform JWTs
```

`Algocura.Platform.Security` does not depend on the Identity API.

This prevents downstream APIs from depending on Identity implementation
details or its database.

---

## Relationship to Other Platform Libraries

```text
Algocura.Platform.Auditing
    Shared entity auditing

Algocura.Platform.Messaging
    Shared Azure Service Bus infrastructure

Algocura.Platform.Security
    Shared HTTP authentication and authorization
```

The three projects serve separate cross-cutting responsibilities.

---

## Recommended Program.cs Pattern

```csharp
// -----------------------------------------------------------------------------
// Security
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraPlatformSecurity(
    builder.Configuration);

// -----------------------------------------------------------------------------
// Auditing
// -----------------------------------------------------------------------------

builder.Services
    .AddAlgocuraAuditing<
        ServiceBusAuditEventPublisher>(
        options =>
        {
            builder.Configuration
                .GetSection(
                    AuditingOptions.SectionName)
                .Bind(options);
        });

// -----------------------------------------------------------------------------
// Azure Service Bus
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);
```

Middleware:

```csharp
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
```
