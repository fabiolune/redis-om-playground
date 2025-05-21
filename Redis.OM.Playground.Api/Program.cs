using Redis.OM.Playground.Api.Configuration;
using Redis.OM.Playground.Api.Endpoints.Extensions;
using Redis.OM.Playground.Api.HostedServices;
using System.Reflection;
using TinyFp.Extensions;

await WebApplication
    .CreateBuilder()
    .Tee(b =>
        b.Services
            .AddRedisConfiguration(b.Configuration)
            .AddOpenApi()
            .AddHostedService<IndexCreationService>()
            .AddEndpointDefinitions(Assembly.GetEntryAssembly()!))
    .Build()
    .UseEndpointDefinitions()
    .RunAsync();