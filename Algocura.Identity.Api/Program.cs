using Algocura.Identity.Api.Data;
using Algocura.Identity.Api.Domain;
using Algocura.Identity.Api.Security;
using Algocura.Platform.Auditing.DependencyInjection;
using Algocura.Platform.Auditing.Interceptors;
using Algocura.Platform.Auditing.Options;
using Algocura.Platform.Messaging.DependencyInjection;
using Algocura.Platform.Messaging.Options;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Controllers and common services
// -----------------------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// -----------------------------------------------------------------------------
// Auditing
// -----------------------------------------------------------------------------

var auditingSection =
    builder.Configuration.GetSection(
        AuditingOptions.SectionName);

builder.Services
    .AddAlgocuraAuditing<
        ServiceBusAuditEventPublisher>(
        options =>
        {
            options.ExcludedPropertyNames.UnionWith(
            [
                "Password",
                "PasswordHash",
                "AccessToken",
                "RefreshToken",
                "ClientSecret",
                "ConnectionString",
                "SharedAccessKey",
                "ApiKey"
            ]);

            options.MaskedPropertyNames.UnionWith(
            [
                "Email",
                "Recipient",
                "IpAddress",
                "UserAgent"
            ]);

            auditingSection.Bind(options);
        });

// -----------------------------------------------------------------------------
// Entity Framework Core
// -----------------------------------------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString(
        "IdentityDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'IdentityDatabase' was not found.");

builder.Services.AddDbContext<IdentityDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            connectionString,
            sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay:
                        TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);

                sqlServerOptions.MigrationsAssembly(
                    typeof(Program).Assembly.FullName);
            });

        options.AddInterceptors(
            serviceProvider.GetRequiredService<
                AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<
                EntityAuditEventInterceptor>());

        if (builder.Environment.IsDevelopment())
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }
    });

// -----------------------------------------------------------------------------
// ASP.NET Core Identity
// -----------------------------------------------------------------------------

builder.Services
    .AddIdentity<
        ApplicationUser,
        ApplicationRole>(
        options =>
        {
            options.User.RequireUniqueEmail =
                true;

            options.Password.RequiredLength =
                12;

            options.Password.RequireDigit =
                true;

            options.Password.RequireLowercase =
                true;

            options.Password.RequireUppercase =
                true;

            options.Password.RequireNonAlphanumeric =
                true;

            options.Lockout.AllowedForNewUsers =
                true;

            options.Lockout.MaxFailedAccessAttempts =
                5;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);
        })
    .AddEntityFrameworkStores<
        IdentityDbContext>()
    .AddDefaultTokenProviders();

// -----------------------------------------------------------------------------
// JWT authentication
// -----------------------------------------------------------------------------

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(
        builder.Configuration.GetSection(
            JwtOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.Issuer),
        "Jwt:Issuer is required.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.Audience),
        "Jwt:Audience is required.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.SigningKey) &&
            options.SigningKey.Length >= 32,
        "Jwt:SigningKey must contain at least 32 characters.")
    .ValidateOnStart();

var jwtOptions =
    builder.Configuration
        .GetSection(
            JwtOptions.SectionName)
        .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "Jwt configuration is required.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer =
                        true,

                    ValidIssuer =
                        jwtOptions.Issuer,

                    ValidateAudience =
                        true,

                    ValidAudience =
                        jwtOptions.Audience,

                    ValidateIssuerSigningKey =
                        true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtOptions.SigningKey)),

                    ValidateLifetime =
                        true,

                    ClockSkew =
                        TimeSpan.FromMinutes(1)
                };
        });

builder.Services.AddAuthorization();

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

// -----------------------------------------------------------------------------
// OpenAPI document generation
// -----------------------------------------------------------------------------

builder.Services.AddOpenApi(
    options =>
    {
        options.AddDocumentTransformer(
            (
                document,
                context,
                cancellationToken) =>
            {
                document.Info.Title =
                    "Algocura Identity API";

                document.Info.Version =
                    "v1";

                document.Info.Description =
                    """
                    The Algocura Identity API provides centralized identity,
                    authentication, authorization, and access-token services
                    for the Algocura Drug Discovery Platform.

                    Capabilities include:

                    - User management
                    - Role management
                    - User-role assignment
                    - JWT access-token issuance
                    - Password management
                    - Account lockout protection
                    - Auditable identity metadata
                    - Entity audit-event publishing
                    - Azure Service Bus integration
                    """;

                document.Info.Contact =
                    new()
                    {
                        Name =
                            "Algocura Platform Engineering"
                    };

                document.Info.License =
                    new()
                    {
                        Name =
                            "Proprietary"
                    };

                return Task.CompletedTask;
            });
    });

// -----------------------------------------------------------------------------
// Azure Service Bus
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

// -----------------------------------------------------------------------------
// Build application
// -----------------------------------------------------------------------------

var app = builder.Build();

// -----------------------------------------------------------------------------
// Database initialization
// -----------------------------------------------------------------------------

await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Configuration,
    app.Logger,
    app.Lifetime.ApplicationStopping);

// -----------------------------------------------------------------------------
// Middleware
// -----------------------------------------------------------------------------

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------------------------
// OpenAPI JSON
//
// /openapi/v1.json
// -----------------------------------------------------------------------------

app.MapOpenApi();

// -----------------------------------------------------------------------------
// Swagger UI
//
// /swagger
// -----------------------------------------------------------------------------

app.UseSwaggerUI(
    options =>
    {
        options.RoutePrefix =
            "swagger";

        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Algocura Identity API v1");

        options.DocumentTitle =
            "Algocura Identity API - Swagger";

        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.EnableTryItOutByDefault();
        options.ShowExtensions();
        options.ShowCommonExtensions();
    });

// -----------------------------------------------------------------------------
// ReDoc
//
// /redoc
// -----------------------------------------------------------------------------

app.UseReDoc(
    options =>
    {
        options.RoutePrefix =
            "redoc";

        options.SpecUrl(
            "/openapi/v1.json");

        options.DocumentTitle =
            "Algocura Identity API - ReDoc";

        options.ExpandResponses(
            "200,201");

        options.RequiredPropsFirst();
    });

// -----------------------------------------------------------------------------
// Endpoints
// -----------------------------------------------------------------------------

app.MapControllers();

app.MapHealthChecks(
        "/health")
    .ExcludeFromDescription();

app.MapGet(
        "/",
        (
            IOptions<ServiceBusOptions> serviceBusOptions,
            IOptions<AuditingOptions> auditingOptions,
            IWebHostEnvironment environment) =>
        {
            var messagingOptions =
                serviceBusOptions.Value;

            var auditOptions =
                auditingOptions.Value;

            return Results.Ok(
                new
                {
                    service =
                        "Algocura Identity API",

                    version =
                        "v1",

                    status =
                        "Running",

                    environment =
                        environment.EnvironmentName,

                    authentication =
                        "JWT Bearer",

                    documentation =
                        new
                        {
                            swagger =
                                "/swagger",

                            redoc =
                                "/redoc",

                            openApi =
                                "/openapi/v1.json"
                        },

                    health =
                        "/health",

                    messaging =
                        new
                        {
                            provider =
                                "Azure Service Bus",

                            mode =
                                messagingOptions.UseEmulator
                                    ? "Emulator"
                                    : "Azure",

                            topic =
                                messagingOptions.TopicName
                        },

                    auditing =
                        new
                        {
                            serviceName =
                                auditOptions.ServiceName,

                            entityStamping =
                                auditOptions
                                    .StampCreatedAndUpdatedFields,

                            eventPublishing =
                                auditOptions
                                    .PublishEntityAuditEvents,

                            includeUnchangedPropertiesOnCreate =
                                auditOptions
                                    .IncludeUnchangedPropertiesOnCreate,

                            includeUnchangedPropertiesOnDelete =
                                auditOptions
                                    .IncludeUnchangedPropertiesOnDelete,

                            ignoreAuditInfrastructureEntities =
                                auditOptions
                                    .IgnoreAuditInfrastructureEntities
                        }
                });
        })
    .WithName(
        "GetServiceInformation")
    .WithSummary(
        "Get service information")
    .WithDescription(
        "Returns service status, authentication type, documentation links, " +
        "messaging configuration, and auditing configuration.")
    .Produces(
        StatusCodes.Status200OK);

// -----------------------------------------------------------------------------
// Start application
// -----------------------------------------------------------------------------

await app.RunAsync();

public partial class Program;