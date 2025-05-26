using Redis.OM;
using Redis.OM.Contracts;
using StackExchange.Redis;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Configuration.Redis;

public static class RedisConfigurationExtensions
{
    public static IServiceCollection AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration) =>
        configuration
            .GetSection(nameof(RedisConfiguration))
            .Get<RedisConfiguration>()
            .ToOption(c => c is null || c.ConnectionString is null)
            .Map(c => c!.ConnectionString!.Tee(Console.WriteLine))
            .Map(c => ConnectionMultiplexer.Connect(c))
            .Map(m => new RedisConnectionProvider(m))
            .Match(services.AddSingleton<IRedisConnectionProvider>,
                () => throw new InvalidOperationException("Redis connection is not configured."));
}