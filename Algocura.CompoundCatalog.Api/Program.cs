using Algocura.CompoundCatalog.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Application services
// -----------------------------------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

// -----------------------------------------------------------------------------
// Entity Framework Core
// -----------------------------------------------------------------------------

builder.Services.AddDbContext<CompoundCatalogDbContext>(options =>
{
    options.UseInMemoryDatabase("AlgocuraCompoundCatalog");
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

                The service supports:

                - Compound registration
                - Molecular metadata management
                - Compound searching and filtering
                - Compound lifecycle tracking
                - Integration with enumeration, screening, docking,
                  ADMET prediction, and candidate-prioritization services

                This API is part of the Algocura cloud-native,
                microservices-based drug-discovery platform.
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
// Build application
// -----------------------------------------------------------------------------

var app = builder.Build();

// -----------------------------------------------------------------------------
// Error handling
// -----------------------------------------------------------------------------

app.UseExceptionHandler();

// -----------------------------------------------------------------------------
// HTTPS
// -----------------------------------------------------------------------------

app.UseHttpsRedirection();

// -----------------------------------------------------------------------------
// OpenAPI JSON
//
// Available at:
// /openapi/v1.json
// -----------------------------------------------------------------------------

app.MapOpenApi();

// -----------------------------------------------------------------------------
// Swagger UI
//
// Available at:
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
// Available at:
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

    options.EnableUntrustedSpec();
});

// -----------------------------------------------------------------------------
// API controllers
// -----------------------------------------------------------------------------

app.MapControllers();

// -----------------------------------------------------------------------------
// Health endpoint
//
// Available at:
// /health
// -----------------------------------------------------------------------------

app.MapHealthChecks("/health")
    .ExcludeFromDescription();

// -----------------------------------------------------------------------------
// Root endpoint
// -----------------------------------------------------------------------------

app.MapGet("/", () => Results.Ok(new
{
    service = "Algocura Compound Catalog API",
    version = "v1",
    status = "Running",
    documentation = new
    {
        swagger = "/swagger",
        redoc = "/redoc",
        openApi = "/openapi/v1.json"
    },
    health = "/health"
}))
    .WithName("GetServiceInformation")
    .WithSummary("Get service information")
    .WithDescription(
        "Returns general information and documentation links for the service.")
    .Produces(StatusCodes.Status200OK);

// -----------------------------------------------------------------------------
// Seed the in-memory database
// -----------------------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var logger = services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseInitialization");

    try
    {
        var dbContext =
            services.GetRequiredService<CompoundCatalogDbContext>();

        await CompoundCatalogSeeder.SeedAsync(dbContext);

        logger.LogInformation(
            "Algocura Compound Catalog in-memory database initialized.");
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

app.Run();