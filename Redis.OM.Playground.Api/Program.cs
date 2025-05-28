using Redis.OM.Playground.Api.Configuration.Redis;
using Redis.OM.Playground.Api.Configuration.Routing;
using Redis.OM.Playground.Api.Endpoints.Extensions;
using Redis.OM.Playground.Api.HostedServices;
using Redis.OM.Playground.Api.Infrastructure;
using System.Reflection;
using TinyFp.Extensions;

await WebApplication
    .CreateBuilder()
    .Tee(b =>
        b.Services
            .AddRoutingConfiguration(b.Configuration)
            .AddRedisConfiguration(b.Configuration)
            .AddExceptionHandler<InternalServerExceptionHandler>()
            .AddProblemDetails()
            .AddOpenApi()
            .AddHostedService<IndexCreationService>()
            .AddEndpointDefinitions(Assembly.GetEntryAssembly()!))
    .Build()
    .Tee(a => a.UseExceptionHandler())
    .UseRoutingBasePath()
    .UseEndpointDefinitions()
    .RunAsync();