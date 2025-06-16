using Redis.OM.Contracts;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Configuration.Redis;

[ExcludeFromCodeCoverage(Justification = "Initialization code that manages a connection to an actual Redis instance")]
public static class RedisConfigurationExtensions
{
    public static IServiceCollection AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration, string name) =>
        configuration
            .GetConnectionString(name)
            .ToOption(string.IsNullOrWhiteSpace)
            .Map(c => ConnectionMultiplexer.Connect(c!).Tee(m => services.AddSingleton<IConnectionMultiplexer>(m)))
            .Map(m => new RedisConnectionProvider(m))
            .Match(services.AddSingleton<IRedisConnectionProvider>,
                () => throw new InvalidOperationException("Redis connection is not configured."))
            .AddSingleton<IDatabaseProvider, DatabaseProvider>();
}
