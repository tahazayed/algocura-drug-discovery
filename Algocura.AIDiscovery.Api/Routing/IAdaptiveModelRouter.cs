namespace Algocura.AIDiscovery.Api.Routing;

public interface IAdaptiveModelRouter
{
    ModelRoutingResult Route(ModelRoutingContext context);
}
