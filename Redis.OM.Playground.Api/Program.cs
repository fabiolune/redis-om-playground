using Redis.OM.Playground.Api.Configuration.Redis;
using Redis.OM.Playground.Api.Configuration.Routing;
using Redis.OM.Playground.Api.Endpoints.Extensions;
using Redis.OM.Playground.Api.GraphQL.Resolvers;
using Redis.OM.Playground.Api.GraphQL.Types;
using Redis.OM.Playground.Api.HostedServices;
using Redis.OM.Playground.Api.Infrastructure;
using Redis.OM.Playground.ServiceDefaults;
using StackExchange.Redis;
using System.Reflection;
using TinyFp.Extensions;

const string CorsPolicyName = "AllowAny";
const string RedisSectionName = "RedisOs";

await WebApplication
    .CreateBuilder()
    .Tee(b =>
        b.Services
            .AddMediator()
            .AddCors(options => options.AddPolicy(CorsPolicyName, b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()))
            .AddRoutingConfiguration(b.Configuration)
            .AddRedisConfiguration(b.Configuration, RedisSectionName)
            .AddExceptionHandler<InternalServerExceptionHandler>()
            .AddProblemDetails()
            .AddOpenApi()
            .AddSingleton(TimeProvider.System)
            .AddHostedService<IndexCreationService>()
            .AddHostedService<TimeSeriesInitializerService>()
            .AddHostedService<TimeTriggerService>()
            .AddSingleton<ITimeSeriesResolver, TimeSeriesResolver>()
            .AddEndpointDefinitions(Assembly.GetEntryAssembly()!)
            .AddGraphQLServer()
            .AddRedisSubscriptions(sp => sp.GetRequiredService<IConnectionMultiplexer>())
            .AddQueryType<Query>()
            .AddSubscriptionType<Subscription>()
            )
    .AddServiceDefaults()
    .Build()
    .Tee(b => b.UseCors(CorsPolicyName))
    .Tee(a => a.UseExceptionHandler())
    .Tee(b => b.UseWebSockets().UseRouting())
    .Tee(b => b.MapGraphQL())
    .UseRoutingBasePath()
    .UseEndpointDefinitions()
    .RunAsync();