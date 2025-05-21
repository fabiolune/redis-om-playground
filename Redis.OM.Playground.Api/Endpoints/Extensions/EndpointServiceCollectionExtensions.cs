using System.Reflection;

namespace Redis.OM.Playground.Api.Endpoints.Extensions;

public static class EndpointServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointDefinitions(this IServiceCollection services, params Type[] scanMarkers) =>
        services.Scan(s => s
            .FromAssembliesOf(scanMarkers)
            .AddClasses(f => f.AssignableTo<IEndpoint>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

    public static IServiceCollection AddEndpointDefinitions(this IServiceCollection services, params Assembly[] assemblies) =>
        services.Scan(s => s
            .FromAssemblies(assemblies)
            .AddClasses(f => f.AssignableTo<IEndpoint>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
}
