using Algocura.LiteratureIntelligence.Api.Data;
using Algocura.LiteratureIntelligence.Api.Services;
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
// CORS
// -----------------------------------------------------------------------------

var allowedOrigins =
    builder.Configuration
        .GetSection(
            "Cors:AllowedOrigins")
        .Get<string[]>()
    ?? [];

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "AlgocuraFrontend",
            policy =>
            {
                policy
                    .WithOrigins(
                        allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

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
        "LiteratureIntelligenceDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:LiteratureIntelligenceDatabase is required.");

builder.Services.AddDbContext<LiteratureIntelligenceDbContext>(
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
    ILiteratureAnalysisService,
    RuleBasedLiteratureAnalysisService>();

// -----------------------------------------------------------------------------
// Azure Service Bus
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

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
                    "Algocura Literature Intelligence API";

                document.Info.Version =
                    "v1";

                document.Info.Description =
                    "Manages publications, disease-gene evidence, " +
                    "literature analysis, and target hypotheses.";

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

app.UseCors(
    "AlgocuraFrontend");

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
            "Algocura Literature Intelligence API v1");

        options.DocumentTitle =
            "Algocura Literature Intelligence API - Swagger";

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
            "Algocura Literature Intelligence API - ReDoc";

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
                        "Algocura Literature Intelligence API",

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
        "Returns service status, documentation links, messaging " +
        "configuration, and auditing configuration.")
    .Produces(
        StatusCodes.Status200OK);

// -----------------------------------------------------------------------------
// Start application
// -----------------------------------------------------------------------------

await app.RunAsync();

public partial class Program;