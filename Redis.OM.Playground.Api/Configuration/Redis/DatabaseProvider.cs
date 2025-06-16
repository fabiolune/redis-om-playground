using StackExchange.Redis;

namespace Redis.OM.Playground.Api.Configuration.Redis;

public class DatabaseProvider(IConnectionMultiplexer connectionMultiplexer) : IDatabaseProvider
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;

    public IDatabase GetDatabase() => _connectionMultiplexer.GetDatabase();
}