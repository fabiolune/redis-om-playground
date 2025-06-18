namespace Redis.OM.Playground.Api.Infrastructure;

public record TimePoint<T>(long Timestamp, T Value)
{
    public DateTimeOffset DateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp);
}
