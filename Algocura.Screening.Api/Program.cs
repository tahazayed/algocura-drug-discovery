using Algocura.Platform.Auditing.DependencyInjection;
using Algocura.Platform.Auditing.Interceptors;
using Algocura.Screening.Api.Data;
using Algocura.Screening.Api.Messaging;
using Algocura.Screening.Api.Messaging.Consumers;
using Algocura.Screening.Api.Messaging.Publishers;
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

// Built-in ASP.NET Core OpenAPI document generation.
// The document will be available at /openapi/v1.json.
builder.Services.AddOpenApi();

// Health check endpoint.
builder.Services.AddHealthChecks();

// -----------------------------------------------------------------------------
// Auditing
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraAuditing(options =>
{
    options.ServiceName =
        "Algocura.Screening.Api";

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
    builder.Configuration.GetConnectionString("ScreeningDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:ScreeningDatabase is required.");

builder.Services.AddDbContext<ScreeningDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            databaseConnectionString,
            sqlServerOptions =>
                sqlServerOptions.EnableRetryOnFailure(
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
// Azure Service Bus configuration
// -----------------------------------------------------------------------------

builder.Services
    .AddOptions<ServiceBusOptions>()
    .Bind(
        builder.Configuration.GetSection(
            ServiceBusOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.TopicName),
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
        "ServiceBus:ConnectionString is required when UseEmulator is true. " +
        "ServiceBus:FullyQualifiedNamespace is required when UseEmulator is false.")
    .ValidateOnStart();

// -----------------------------------------------------------------------------
// Azure Service Bus client
// -----------------------------------------------------------------------------

builder.Services.AddSingleton<ServiceBusClient>(
    serviceProvider =>
    {
        var options =
            serviceProvider
                .GetRequiredService<IOptions<ServiceBusOptions>>()
                .Value;

        if (options.UseEmulator)
        {
            return new ServiceBusClient(
                options.ConnectionString);
        }

        return new ServiceBusClient(
            options.FullyQualifiedNamespace,
            new DefaultAzureCredential());
    });

builder.Services.AddSingleton<
    IEventPublisher,
    ServiceBusEventPublisher>();

builder.Services.AddHostedService<
    CompoundEventsConsumer>();

// -----------------------------------------------------------------------------
// Application pipeline
// -----------------------------------------------------------------------------

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Built-in OpenAPI JSON:
    // /openapi/v1.json
    app.MapOpenApi();

    // Swagger UI uses the built-in OpenAPI document.
    app.UseSwaggerUI(
        options =>
        {
            options.RoutePrefix = "swagger";

            options.SwaggerEndpoint(
                "/openapi/v1.json",
                "Algocura Screening API v1");

            options.DocumentTitle =
                "Algocura Screening API";
        });

    // ReDoc uses the same built-in OpenAPI document.
    app.UseReDoc(
        options =>
        {
            options.RoutePrefix = "redoc";
            options.SpecUrl = "/openapi/v1.json";

            options.DocumentTitle =
                "Algocura Screening API Documentation";
        });
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

// -----------------------------------------------------------------------------
// Root service information endpoint
// -----------------------------------------------------------------------------

app.MapGet(
    "/",
    (
        IHostEnvironment environment,
        IOptions<ServiceBusOptions> serviceBusOptions) =>
    {
        var options = serviceBusOptions.Value;

        return Results.Ok(
            new
            {
                service = "Algocura Screening API",
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
                    mode = options.UseEmulator
                        ? "Emulator"
                        : "Azure",
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

// -----------------------------------------------------------------------------
// Database initialization
// -----------------------------------------------------------------------------

await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Logger,
    app.Lifetime.ApplicationStopping);

await app.RunAsync();

public partial class Program;