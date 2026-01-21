using Algocura.CompoundCatalog.Api.Data;
using Algocura.Platform.Auditing.DependencyInjection;
using Algocura.Platform.Auditing.Interceptors;
using Algocura.Platform.Auditing.Options;
using Algocura.Platform.Messaging.DependencyInjection;
using Algocura.Platform.Messaging.Options;
using Algocura.Platform.Messaging.Publishing;
using Algocura.Platform.Security.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Controllers and common services
// -----------------------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// -----------------------------------------------------------------------------
// Security
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraPlatformSecurity(
    builder.Configuration);

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
        "CompoundCatalogDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'CompoundCatalogDatabase' was not found.");

builder.Services.AddDbContext<CompoundCatalogDbContext>(
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
                    "Algocura Compound Catalog API";

                document.Info.Version =
                    "v1";

                document.Info.Description =
                    """
                    The Algocura Compound Catalog API manages chemical compounds
                    used throughout the drug-discovery lifecycle.

                    Capabilities include:

                    - Compound registration
                    - Molecular metadata management
                    - Compound searching and filtering
                    - Compound status management
                    - Auditable entity metadata
                    - Integration-event publishing
                    - Azure Service Bus integration
                    - Integration with screening, docking, ADMET, and
                      candidate-prioritization services
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
            "Algocura Compound Catalog API v1");

        options.DocumentTitle =
            "Algocura Compound Catalog API - Swagger";

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
            "Algocura Compound Catalog API - ReDoc";

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
                        "Algocura Compound Catalog API",

                    version =
                        "v1",

                    status =
                        "Running",

                    environment =
                        environment.EnvironmentName,

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
        "Returns service status, documentation links, " +
        "messaging configuration, and auditing configuration.")
    .Produces(
        StatusCodes.Status200OK);

// -----------------------------------------------------------------------------
// Start application
// -----------------------------------------------------------------------------

await app.RunAsync();

public partial class Program;
