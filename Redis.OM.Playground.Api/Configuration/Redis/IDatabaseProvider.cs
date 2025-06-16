using StackExchange.Redis;

namespace Redis.OM.Playground.Api.Configuration.Redis;

public interface IDatabaseProvider
{
    IDatabase GetDatabase();
}
