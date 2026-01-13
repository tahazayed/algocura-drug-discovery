using Algocura.ChemicalDesign.Api.Data;
using Algocura.ChemicalDesign.Api.Messaging;
using Algocura.ChemicalDesign.Api.Messaging.Consumers;
using Algocura.ChemicalDesign.Api.Messaging.Publishers;
using Algocura.ChemicalDesign.Api.Services;
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
        document.Info.Title = "Algocura Chemical Design API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Manages selected targets, design projects, query structures, " +
            "prototype structure searches, candidates, synthesis routes, " +
            "and compound-registration handoff.";
        document.Info.Contact = new() { Name = "Algocura Platform Engineering" };
        document.Info.License = new() { Name = "Proprietary" };
        return Task.CompletedTask;
    });
});

var connectionString =
    builder.Configuration.GetConnectionString("ChemicalDesignDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:ChemicalDesignDatabase is required.");

builder.Services.AddDbContext<ChemicalDesignDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sql => sql.EnableRetryOnFailure(
            5,
            TimeSpan.FromSeconds(10),
            null)));

builder.Services.AddScoped<IStructureSearchEngine, DeterministicStructureSearchEngine>();

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
builder.Services.AddHostedService<TargetDiscoveryEventsConsumer>();

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
            "Algocura Chemical Design API v1");
        options.DocumentTitle = "Algocura Chemical Design API";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.EnableTryItOutByDefault();
    });

    app.UseReDoc(options =>
    {
        options.RoutePrefix = "redoc";
        options.SpecUrl("/openapi/v1.json");
        options.DocumentTitle = "Algocura Chemical Design API Documentation";
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
            service = "Algocura Chemical Design API",
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

await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Logger,
    app.Lifetime.ApplicationStopping);

await app.RunAsync();

public partial class Program;
