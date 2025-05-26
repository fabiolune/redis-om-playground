using Redis.OM;
using Redis.OM.Contracts;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Configuration.Redis;

[ExcludeFromCodeCoverage(Justification = "Initialization code that manages a connection to an actual Reids instance")]
public static class RedisConfigurationExtensions
{
    public static IServiceCollection AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration) =>
        configuration
            .GetSection(nameof(RedisConfiguration))
            .Get<RedisConfiguration>()
            .ToOption(c => c is null || c.ConnectionString is null)
            .Map(c => c!.ConnectionString!)
            .Map(c => ConnectionMultiplexer.Connect(c))
            .Map(m => new RedisConnectionProvider(m))
            .Match(services.AddSingleton<IRedisConnectionProvider>,
                () => throw new InvalidOperationException("Redis connection is not configured."));
}