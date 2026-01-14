using Algocura.AssayManagement.Api.Data;
using Algocura.AssayManagement.Api.Messaging;
using Algocura.AssayManagement.Api.Messaging.Consumers;
using Algocura.AssayManagement.Api.Messaging.Publishers;
using Algocura.AssayManagement.Api.Services;
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
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Algocura Assay Management API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Manages completed-screening projections, assays, measurements, " +
            "replicates, quality control, assay completion, and downstream synchronization.";
        document.Info.Contact = new()
        {
            Name = "Algocura Platform Engineering"
        };
        document.Info.License = new()
        {
            Name = "Proprietary"
        };

        return Task.CompletedTask;
    });
});

// -----------------------------------------------------------------------------
// Database
// -----------------------------------------------------------------------------

var databaseConnectionString =
    builder.Configuration.GetConnectionString("AssayManagementDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:AssayManagementDatabase is required.");

builder.Services.AddDbContext<AssayManagementDbContext>(
    options =>
        options.UseSqlServer(
            databaseConnectionString,
            sqlServerOptions =>
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)));

// -----------------------------------------------------------------------------
// Application services
// -----------------------------------------------------------------------------

builder.Services.AddScoped<
    IAssayResultCalculator,
    DeterministicAssayResultCalculator>();

// -----------------------------------------------------------------------------
// Azure Service Bus
// -----------------------------------------------------------------------------

builder.Services
    .AddOptions<ServiceBusOptions>()
    .Bind(builder.Configuration.GetSection(ServiceBusOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.TopicName),
        "ServiceBus:TopicName is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.SubscriptionName),
        "ServiceBus:SubscriptionName is required.")
    .Validate(
        options =>
            options.UseEmulator
                ? !string.IsNullOrWhiteSpace(options.ConnectionString)
                : !string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace),
        "ServiceBus:ConnectionString is required when UseEmulator is true. " +
        "ServiceBus:FullyQualifiedNamespace is required when UseEmulator is false.")
    .ValidateOnStart();

builder.Services.AddSingleton<ServiceBusClient>(
    serviceProvider =>
    {
        var options =
            serviceProvider
                .GetRequiredService<IOptions<ServiceBusOptions>>()
                .Value;

        return options.UseEmulator
            ? new ServiceBusClient(options.ConnectionString)
            : new ServiceBusClient(
                options.FullyQualifiedNamespace,
                new DefaultAzureCredential());
    });

builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
builder.Services.AddHostedService<ScreeningEventsConsumer>();

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
        options.RoutePrefix = "swagger";
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Algocura Assay Management API v1");
        options.DocumentTitle =
            "Algocura Assay Management API - Swagger";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.EnableTryItOutByDefault();
    });

    app.UseReDoc(options =>
    {
        options.RoutePrefix = "redoc";
        options.SpecUrl("/openapi/v1.json");
        options.DocumentTitle =
            "Algocura Assay Management API - ReDoc";
        options.ExpandResponses("200,201");
        options.RequiredPropsFirst();
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health").ExcludeFromDescription();

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
                    service = "Algocura Assay Management API",
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
