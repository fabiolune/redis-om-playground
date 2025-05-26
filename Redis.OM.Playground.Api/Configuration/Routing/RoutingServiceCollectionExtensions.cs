using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Configuration.Routing;

public static class RoutingServiceCollectionExtensions
{
    public static IServiceCollection AddRoutingConfiguration(this IServiceCollection services, IConfiguration configuration) =>
        configuration
            .GetSection(nameof(RoutingConfiguration))
            .Get<RoutingConfiguration>()
            .ToOption(c => c is null || string.IsNullOrEmpty(c.PathBase))
            .Map(c => c!)
            .OrElse(RoutingConfiguration.Default)
            .Map(services.AddSingleton);
}