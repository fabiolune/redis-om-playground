using Redis.OM.Playground.Api.Configuration.Redis;
using Redis.OM.Playground.Api.Configuration.Routing;
using Redis.OM.Playground.Api.Endpoints.Extensions;
using Redis.OM.Playground.Api.HostedServices;
using System.Reflection;
using TinyFp.Extensions;

await WebApplication
    .CreateBuilder()
    .Tee(b =>
        b.Services
            .AddRoutingConfiguration(b.Configuration)
            .AddRedisConfiguration(b.Configuration)
            .AddOpenApi()
            .AddHostedService<IndexCreationService>()
            .AddEndpointDefinitions(Assembly.GetEntryAssembly()!))
    .Build()
    .UseRoutingBasePath()
    .UseEndpointDefinitions()
    .RunAsync();