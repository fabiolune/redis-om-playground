namespace Redis.OM.Playground.Api.Configuration.Redis;

public record RedisConfiguration
{
    public string? ConnectionString { get; init; }
}
