using Algocura.CompoundCatalog.Api.Data;
using Algocura.CompoundCatalog.Api.Messaging;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
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
// Entity Framework Core
// -----------------------------------------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString("CompoundCatalogDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'CompoundCatalogDatabase' was not found.");

builder.Services.AddDbContext<CompoundCatalogDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);

            sqlServerOptions.MigrationsAssembly(
                typeof(Program).Assembly.FullName);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});
// -----------------------------------------------------------------------------
// OpenAPI document generation
// -----------------------------------------------------------------------------

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info.Title = "Algocura Compound Catalog API";
            document.Info.Version = "v1";

            document.Info.Description =
                """
                The Algocura Compound Catalog API manages chemical compounds
                used throughout the drug-discovery lifecycle.

                Capabilities include:

                - Compound registration
                - Molecular metadata management
                - Compound searching and filtering
                - Compound status management
                - Integration-event publishing
                - Azure Service Bus integration
                - Integration with screening, docking, ADMET, and
                  candidate-prioritization services
                """;

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
// Azure Service Bus options
// -----------------------------------------------------------------------------

builder.Services
    .AddOptions<ServiceBusOptions>()
    .Bind(builder.Configuration.GetSection(ServiceBusOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.TopicName),
        "ServiceBus:TopicName is required.")
    .Validate(
        options =>
            options.UseEmulator
                ? !string.IsNullOrWhiteSpace(options.ConnectionString)
                : !string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace),
        "ServiceBus:ConnectionString is required for the emulator, or " +
        "ServiceBus:FullyQualifiedNamespace is required for Azure.")
    .ValidateOnStart();

// ServiceBusClient is registered once and reused for the application lifetime.
// Azure SDK messaging clients are intended to be shared rather than created
// for each request or message. :contentReference[oaicite:0]{index=0}

builder.Services.AddSingleton<ServiceBusClient>(serviceProvider =>
{
    var options = serviceProvider
        .GetRequiredService<IOptions<ServiceBusOptions>>()
        .Value;

    if (options.UseEmulator)
    {
        return new ServiceBusClient(options.ConnectionString);
    }

    return new ServiceBusClient(
        options.FullyQualifiedNamespace,
        new DefaultAzureCredential());
});

// Register the event publisher once.

builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();

// -----------------------------------------------------------------------------
// Build application
// -----------------------------------------------------------------------------

var app = builder.Build();
// -----------------------------------------------------------------------------
// Seed database
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

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";

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

app.UseReDoc(options =>
{
    options.RoutePrefix = "redoc";
    options.SpecUrl("/openapi/v1.json");

    options.DocumentTitle =
        "Algocura Compound Catalog API - ReDoc";

    options.ExpandResponses("200,201");
    options.RequiredPropsFirst();
});

// -----------------------------------------------------------------------------
// Endpoints
// -----------------------------------------------------------------------------

app.MapControllers();

app.MapHealthChecks("/health")
    .ExcludeFromDescription();

app.MapGet(
        "/",
        (
            IOptions<ServiceBusOptions> serviceBusOptions,
            IWebHostEnvironment environment) =>
        {
            var messagingOptions = serviceBusOptions.Value;

            return Results.Ok(new
            {
                service = "Algocura Compound Catalog API",
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
                    mode = messagingOptions.UseEmulator
                        ? "Emulator"
                        : "Azure",
                    topic = messagingOptions.TopicName
                }
            });
        })
    .WithName("GetServiceInformation")
    .WithSummary("Get service information")
    .WithDescription(
        "Returns service status, documentation links, and messaging configuration.")
    .Produces(StatusCodes.Status200OK);

// -----------------------------------------------------------------------------
// Initialize and seed the in-memory database
// -----------------------------------------------------------------------------

await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;

    var logger = services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseInitialization");

    try
    {
        var dbContext =
            services.GetRequiredService<CompoundCatalogDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        await CompoundCatalogSeeder.SeedAsync(dbContext);

        logger.LogInformation(
            "The Algocura Compound Catalog database was initialized.");
    }
    catch (Exception exception)
    {
        logger.LogCritical(
            exception,
            "An error occurred while initializing the database.");

        throw;
    }
}

// -----------------------------------------------------------------------------
// Start application
// -----------------------------------------------------------------------------

await app.RunAsync();
