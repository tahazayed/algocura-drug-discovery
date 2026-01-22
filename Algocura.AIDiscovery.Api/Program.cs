using Algocura.AIDiscovery.Api.Data;
using Algocura.AIDiscovery.Api.Messaging.Consumers;
using Algocura.AIDiscovery.Api.Routing;
using Algocura.AIDiscovery.Api.Services;
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

var databaseConnectionString =
    builder.Configuration.GetConnectionString(
        "AIDiscoveryDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:AIDiscoveryDatabase is required.");

builder.Services.AddDbContext<AIDiscoveryDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            databaseConnectionString,
            sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount:
                        5,

                    maxRetryDelay:
                        TimeSpan.FromSeconds(10),

                    errorNumbersToAdd:
                        null);

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
        }
    });

// -----------------------------------------------------------------------------
// Application services
// -----------------------------------------------------------------------------

builder.Services.AddScoped<
    IPredictionScoringEngine,
    DeterministicPredictionScoringEngine>();

builder.Services
    .AddOptions<ModelRoutingOptions>()
    .Bind(builder.Configuration.GetSection(ModelRoutingOptions.SectionName))
    .Validate(options => options.Models.Count > 0,
        "AdaptiveRouting:Models must contain at least one model profile.")
    .ValidateOnStart();

builder.Services.AddScoped<IAdaptiveModelRouter, AdaptiveModelRouter>();
builder.Services.AddScoped<IAdaptivePredictionOrchestrator, AdaptivePredictionOrchestrator>();

// -----------------------------------------------------------------------------
// Azure Service Bus
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

builder.Services.AddHostedService<
    PlatformEventsConsumer>();

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
                    "Algocura AI Discovery API";

                document.Info.Version =
                    "v1";

                document.Info.Description =
                    "Manages prediction jobs, adaptive molecular-model routing, " +
                    "confidence-driven model escalation, resource-aware " +
                    "candidate prioritization, screening projections, and " +
                    "integration events.";

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
            "Algocura AI Discovery API v1");

        options.DocumentTitle =
            "Algocura AI Discovery API - Swagger";

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
            "Algocura AI Discovery API - ReDoc";

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
                        "Algocura AI Discovery API",

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
                                messagingOptions.TopicName,

                            subscription =
                                messagingOptions.SubscriptionName
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
        "Returns service status, documentation links, messaging " +
        "configuration, and auditing configuration.")
    .Produces(
        StatusCodes.Status200OK);

// -----------------------------------------------------------------------------
// Start application
// -----------------------------------------------------------------------------

await app.RunAsync();

public partial class Program;