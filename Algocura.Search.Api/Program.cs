using Algocura.Platform.Auditing.DependencyInjection;
using Algocura.Platform.Auditing.Interceptors;
using Algocura.Search.Api.Data;
using Algocura.Search.Api.Messaging;
using Algocura.Search.Api.Messaging.Consumers;
using Algocura.Search.Api.Messaging.Publishers;
using Algocura.Search.Api.Services;
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
        document.Info.Title = "Algocura Search API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Provides unified search across platform-owned search projections.";
        return Task.CompletedTask;
    });
});

// -----------------------------------------------------------------------------
// Auditing
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraAuditing(options =>
{
    options.ServiceName = "Algocura.Search.Api";
    options.StampCreatedAndUpdatedFields = true;
    options.PublishEntityAuditEvents = true;
});

var connectionString =
    builder.Configuration.GetConnectionString("SearchDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:SearchDatabase is required.");

builder.Services.AddDbContext<SearchDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            connectionString,
            sql =>
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null));

        options.AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<EntityAuditEventInterceptor>());

        if (builder.Environment.IsDevelopment())
        {
            options.EnableDetailedErrors();
        }
    });

builder.Services.AddScoped<ISearchService, SearchService>();

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
builder.Services.AddHostedService<PlatformSearchEventsConsumer>();

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
            "Algocura Search API v1");
        options.DocumentTitle = "Algocura Search API";
        options.DisplayRequestDuration();
        options.EnableFilter();
        options.EnableTryItOutByDefault();
    });

    app.UseReDoc(options =>
    {
        options.RoutePrefix = "redoc";
        options.SpecUrl("/openapi/v1.json");
        options.DocumentTitle = "Algocura Search API Documentation";
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health").ExcludeFromDescription();

app.MapGet("/", (
        IHostEnvironment environment,
        IOptions<ServiceBusOptions> serviceBusOptions) =>
{
    var options = serviceBusOptions.Value;
    return Results.Ok(new
    {
        service = "Algocura Search API",
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

