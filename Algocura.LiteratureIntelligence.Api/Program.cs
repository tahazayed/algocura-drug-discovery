using Algocura.LiteratureIntelligence.Api.Data;
using Algocura.LiteratureIntelligence.Api.Messaging;
using Algocura.LiteratureIntelligence.Api.Messaging.Publishers;
using Algocura.LiteratureIntelligence.Api.Services;
using Algocura.Platform.Auditing.DependencyInjection;
using Algocura.Platform.Auditing.Interceptors;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// API
// -----------------------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info.Title =
                "Algocura Literature Intelligence API";

            document.Info.Version =
                "v1";

            document.Info.Description =
                "Manages publications, disease-gene evidence, " +
                "literature analysis, and target hypotheses.";

            return Task.CompletedTask;
        });
});

// -----------------------------------------------------------------------------
// Auditing
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraAuditing(
    options =>
    {
        options.ServiceName =
            "Algocura.LiteratureIntelligence.Api";

        options.StampCreatedAndUpdatedFields =
            true;

        // Keep disabled until a Service Bus audit publisher or
        // transactional outbox is registered.
        options.PublishEntityAuditEvents =
            true;
    });

// -----------------------------------------------------------------------------
// Database
// -----------------------------------------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString(
        "LiteratureIntelligenceDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:LiteratureIntelligenceDatabase is required.");

builder.Services.AddDbContext<LiteratureIntelligenceDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            connectionString,
            sql =>
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay:
                        TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null));

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

builder.Services
    .AddOptions<ServiceBusOptions>()
    .Bind(
        builder.Configuration.GetSection(
            ServiceBusOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.TopicName),
        "ServiceBus:TopicName is required.")
    .Validate(
        options =>
            options.UseEmulator
                ? !string.IsNullOrWhiteSpace(
                    options.ConnectionString)
                : !string.IsNullOrWhiteSpace(
                    options.FullyQualifiedNamespace),
        "Configure an emulator connection string or Azure namespace.")
    .ValidateOnStart();

builder.Services.AddSingleton<ServiceBusClient>(
    serviceProvider =>
    {
        var options =
            serviceProvider
                .GetRequiredService<
                    IOptions<ServiceBusOptions>>()
                .Value;

        return options.UseEmulator
            ? new ServiceBusClient(
                options.ConnectionString)
            : new ServiceBusClient(
                options.FullyQualifiedNamespace,
                new DefaultAzureCredential());
    });

builder.Services.AddSingleton<
    IEventPublisher,
    ServiceBusEventPublisher>();

// -----------------------------------------------------------------------------
// Build application
// -----------------------------------------------------------------------------

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix =
            "swagger";

        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Algocura Literature Intelligence API v1");

        options.DocumentTitle =
            "Algocura Literature Intelligence API";

        options.DisplayRequestDuration();
        options.EnableFilter();
        options.EnableTryItOutByDefault();
    });

    app.UseReDoc(options =>
    {
        options.RoutePrefix =
            "redoc";

        options.SpecUrl(
            "/openapi/v1.json");

        options.DocumentTitle =
            "Algocura Literature Intelligence API Documentation";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks(
        "/health")
    .ExcludeFromDescription();

app.MapGet(
        "/",
        (
            IHostEnvironment environment,
            IOptions<ServiceBusOptions> serviceBusOptions) =>
        {
            var options =
                serviceBusOptions.Value;

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
                                options.UseEmulator
                                    ? "Emulator"
                                    : "Azure",

                            topic =
                                options.TopicName
                        },

                    auditing =
                        new
                        {
                            entityStamping =
                                true,

                            eventPublishing =
                                false
                        }
                });
        })
    .WithName(
        "GetServiceInformation")
    .ExcludeFromDescription();

// -----------------------------------------------------------------------------
// Database initialization
// -----------------------------------------------------------------------------

await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Logger,
    app.Lifetime.ApplicationStopping);

await app.RunAsync();

public partial class Program;
