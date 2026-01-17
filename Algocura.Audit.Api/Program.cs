using Algocura.Audit.Api.Data;
using Algocura.Audit.Api.Messaging;
using Algocura.Audit.Api.Services;
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
                "Algocura Audit API";

            document.Info.Version =
                "v1";

            document.Info.Description =
                "Central immutable audit-query service for entity audit events " +
                "emitted by Algocura platform microservices.";

            document.Info.Contact = new()
            {
                Name =
                    "Algocura Platform Engineering"
            };

            document.Info.License = new()
            {
                Name =
                    "Proprietary"
            };

            return Task.CompletedTask;
        });
});

// -----------------------------------------------------------------------------
// Auditing
// -----------------------------------------------------------------------------
// The Audit API stamps its own persistence metadata but does not publish
// EntityAuditEvent messages for AuditRecord persistence. This avoids an
// audit-event feedback loop.

builder.Services.AddAlgocuraAuditing(options =>
{
    options.ServiceName =
        "Algocura.Audit.Api";

    options.StampCreatedAndUpdatedFields =
        true;

    options.PublishEntityAuditEvents =
        true;

    options.IgnoreAuditInfrastructureEntities =
        true;
});

// -----------------------------------------------------------------------------
// Database
// -----------------------------------------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString(
        "AuditDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:AuditDatabase is required.");

builder.Services.AddDbContext<AuditDbContext>(
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
    IAuditIngestionService,
    AuditIngestionService>();

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
            !string.IsNullOrWhiteSpace(
                options.SubscriptionName),
        "ServiceBus:SubscriptionName is required.")
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

builder.Services.AddHostedService<
    EntityAuditEventsConsumer>();

// -----------------------------------------------------------------------------
// Application pipeline
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
            "Algocura Audit API v1");

        options.DocumentTitle =
            "Algocura Audit API";

        options.DisplayRequestDuration();
        options.EnableDeepLinking();
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
            "Algocura Audit API Documentation";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/health")
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
                        "Algocura Audit API",

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
                                options.TopicName,

                            subscription =
                                options.SubscriptionName,

                            consumedEvent =
                                "EntityAuditEvent"
                        },

                    auditing =
                        new
                        {
                            entityStamping =
                                true,

                            eventPublishing =
                                false,

                            feedbackLoopProtection =
                                true
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
