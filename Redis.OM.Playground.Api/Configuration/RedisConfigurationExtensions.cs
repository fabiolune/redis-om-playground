using Redis.OM.Contracts;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Configuration;

public static class RedisConfigurationExtensions
{
    public static IServiceCollection AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration) =>
        configuration
            .GetSection(nameof(RedisConfiguration))
            .Get<RedisConfiguration>()
            .ToOption(c => c is null || c.ConnectionString is null)
            .Match(
                c => services.AddSingleton<IRedisConnectionProvider>(new RedisConnectionProvider(c!.ConnectionString!)),
                () => throw new InvalidOperationException("Redis connection string is not configured."));
}