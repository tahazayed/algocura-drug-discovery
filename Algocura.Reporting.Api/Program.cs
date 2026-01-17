using Algocura.Platform.Auditing.DependencyInjection;
using Algocura.Platform.Auditing.Interceptors;
using Algocura.Reporting.Api.Data;
using Algocura.Reporting.Api.Messaging;
using Algocura.Reporting.Api.Services;
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
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info.Title =
                "Algocura Reporting API";
            document.Info.Version = "v1";
            document.Info.Description =
                "Provides reporting definitions, report execution, " +
                "report metrics, platform summaries, and reporting projections.";
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
// Auditing
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraAuditing(options =>
{
    options.ServiceName =
        "Algocura.Reporting.Api";
    options.StampCreatedAndUpdatedFields =
        true;
    options.PublishEntityAuditEvents =
        true;
});

// -----------------------------------------------------------------------------
// Database
// -----------------------------------------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString(
        "ReportingDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:ReportingDatabase is required.");

builder.Services.AddDbContext<ReportingDbContext>(
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

builder.Services.AddScoped<
    IReportExecutionService,
    ReportExecutionService>();

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
            "Algocura Reporting API v1");
        options.DocumentTitle =
            "Algocura Reporting API";
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
            "Algocura Reporting API Documentation";
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
            var options = serviceBusOptions.Value;

            return Results.Ok(
                new
                {
                    service =
                        "Algocura Reporting API",
                    version =
                        "v1",
                    status =
                        "Running",
                    environment =
                        environment.EnvironmentName,
                    documentation =
                        new
                        {
                            swagger = "/swagger",
                            redoc = "/redoc",
                            openApi = "/openapi/v1.json"
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
                                options.SubscriptionName
                        },
                    auditing =
                        new
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
