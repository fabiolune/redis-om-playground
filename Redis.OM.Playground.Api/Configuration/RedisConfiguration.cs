namespace Redis.OM.Playground.Api.Configuration;

public record RedisConfiguration
{
    public string? ConnectionString { get; init; }
}
