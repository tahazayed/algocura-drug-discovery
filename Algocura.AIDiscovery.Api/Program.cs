using Algocura.AIDiscovery.Api.Data;
using Algocura.AIDiscovery.Api.Messaging;
using Algocura.AIDiscovery.Api.Messaging.Consumers;
using Algocura.AIDiscovery.Api.Messaging.Publishers;
using Algocura.AIDiscovery.Api.Services;
using Algocura.Platform.Auditing.DependencyInjection;
using Algocura.Platform.Auditing.Interceptors;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Algocura AI Discovery API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Manages prediction jobs, deterministic prototype scoring, " +
            "candidate prioritization, and integration events.";
        document.Info.Contact = new() { Name = "Algocura Platform Engineering" };
        document.Info.License = new() { Name = "Proprietary" };
        return Task.CompletedTask;
    });
});

// -----------------------------------------------------------------------------
// Auditing
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraAuditing(options =>
{
    options.ServiceName =
        "Algocura.AIDiscovery.Api";

    options.StampCreatedAndUpdatedFields =
        true;

    // Enable after registering a Service Bus audit publisher
    // or implementing a transactional outbox.
    options.PublishEntityAuditEvents =
        false;
});

// -----------------------------------------------------------------------------
// Database
// -----------------------------------------------------------------------------

var databaseConnectionString =
    builder.Configuration.GetConnectionString("AIDiscoveryDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:AIDiscoveryDatabase is required.");

builder.Services.AddDbContext<AIDiscoveryDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            databaseConnectionString,
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

builder.Services.AddScoped<
    IPredictionScoringEngine,
    DeterministicPredictionScoringEngine>();

builder.Services
    .AddOptions<ServiceBusOptions>()
    .Bind(builder.Configuration.GetSection(ServiceBusOptions.SectionName))
    .Validate(x => !string.IsNullOrWhiteSpace(x.TopicName),
        "ServiceBus:TopicName is required.")
    .Validate(x => !string.IsNullOrWhiteSpace(x.SubscriptionName),
        "ServiceBus:SubscriptionName is required.")
    .Validate(
        x => x.UseEmulator
            ? !string.IsNullOrWhiteSpace(x.ConnectionString)
            : !string.IsNullOrWhiteSpace(x.FullyQualifiedNamespace),
        "Configure an emulator connection string or Azure namespace.")
    .ValidateOnStart();

builder.Services.AddSingleton<ServiceBusClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;

    return options.UseEmulator
        ? new ServiceBusClient(options.ConnectionString)
        : new ServiceBusClient(
            options.FullyQualifiedNamespace,
            new DefaultAzureCredential());
});

builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
builder.Services.AddHostedService<PlatformEventsConsumer>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger";
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Algocura AI Discovery API v1");
        options.DocumentTitle = "Algocura AI Discovery API - Swagger";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.EnableTryItOutByDefault();
    });

    app.UseReDoc(options =>
    {
        options.RoutePrefix = "redoc";
        options.SpecUrl("/openapi/v1.json");
        options.DocumentTitle = "Algocura AI Discovery API - ReDoc";
        options.ExpandResponses("200,201");
        options.RequiredPropsFirst();
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/health")
    .ExcludeFromDescription();

app.MapGet("/", (
        IHostEnvironment environment,
        IOptions<ServiceBusOptions> serviceBusOptions) =>
{
    var options = serviceBusOptions.Value;

    return Results.Ok(new
    {
        service = "Algocura AI Discovery API",
        version = "v1",
        status = "Running",
        environment = environment.EnvironmentName,
        documentation = new
        {
            swagger = "/swagger",
            redoc = "/redoc",
            openApi = "/openapi/v1.json"
        },
        health = "/health",
        messaging = new
        {
            provider = "Azure Service Bus",
            mode = options.UseEmulator ? "Emulator" : "Azure",
            topic = options.TopicName,
            subscription = options.SubscriptionName
        },
        auditing = new
        {
            entityStamping = true,
            eventPublishing = false
        }
    });
})
    .WithName("GetServiceInformation")
    .ExcludeFromDescription();

await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Logger,
    app.Lifetime.ApplicationStopping);

await app.RunAsync();

public partial class Program;
