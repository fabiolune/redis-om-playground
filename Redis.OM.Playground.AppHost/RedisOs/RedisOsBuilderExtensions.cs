using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TinyFp.Extensions;

namespace Redis.OM.Playground.AppHost.RedisOs;
public static class RedisOsBuilderExtensions
{
    private const string RedisOsContainerImageTags = "8-alpine";
    private const string RedisOsContainerImage = "redis";
    private const string RedisOsContainerRegistry = "docker.io";

    public static IResourceBuilder<RedisResource> AddRedisOs(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        string image = RedisOsContainerImage,
        string tag = RedisOsContainerImageTags,
        string registry = RedisOsContainerRegistry,
        int? port = null,
        IResourceBuilder<ParameterResource>? password = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var redis = new RedisResource(name, password?.Resource!);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(redis, async (_, ct) =>
        {
            connectionString = await redis.GetConnectionStringAsync(ct).ConfigureAwait(false);
            connectionString
                .ToEither(redis.Name)
                .OnLeft(name => throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{name}' resource but the connection string was not available."));
        });

        var healthCheckKey = $"{name}_check";

        return builder
            .Tee(b => b.Services
                .AddHealthChecks()
                .AddRedis(sp => connectionString.Map(c => ConnectionMultiplexer.Connect(c!)), name: healthCheckKey))
            .AddResource(redis)
            .WithEndpoint(port: port, targetPort: 6379, name: "tcp")
            .WithImage(image, tag)
            .WithImageRegistry(registry)
            .WithHealthCheck(healthCheckKey)
            .WithEnvironment(context =>
            {
                if (redis.PasswordParameter is { } password)
                {
                    context.EnvironmentVariables["REDIS_PASSWORD"] = password.Value;
                }
            })
            .WithArgs(context =>
            {
                var redisCommand = new List<string>
                {
                    "redis-server"
                };

                if (redis.PasswordParameter is not null)
                {
                    redisCommand.Add("--requirepass");
                    redisCommand.Add("$REDIS_PASSWORD");
                }

                redisCommand.ForEach(arg => context.Args.Add(arg));

                return Task.CompletedTask;
            });
    }
}
