using Redis.OM.Playground.Api.Configuration.Redis;
using Redis.OM.Playground.Api.Configuration.Routing;
using Redis.OM.Playground.Api.Endpoints.Extensions;
using Redis.OM.Playground.Api.HostedServices;
using Redis.OM.Playground.Api.Infrastructure;
using Redis.OM.Playground.ServiceDefaults;
using System.Reflection;
using TinyFp.Extensions;

const string CorsPolicyName = "AllowAny";

await WebApplication
    .CreateBuilder()
    .Tee(b =>
        b.Services
            .AddCors(options => options.AddPolicy(CorsPolicyName, b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()))
            .AddRoutingConfiguration(b.Configuration)
            .AddRedisConfiguration(b.Configuration, "RedisOs")
            .AddExceptionHandler<InternalServerExceptionHandler>()
            .AddProblemDetails()
            .AddOpenApi()
            .AddHostedService<IndexCreationService>()
            .AddEndpointDefinitions(Assembly.GetEntryAssembly()!))
    .AddServiceDefaults()
    .Build()
    .Tee(b => b.UseCors(CorsPolicyName))
    .Tee(a => a.UseExceptionHandler())
    .UseRoutingBasePath()
    .UseEndpointDefinitions()
    .RunAsync();