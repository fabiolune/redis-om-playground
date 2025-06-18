namespace Redis.OM.Playground.Api.Infrastructure;

public record TimeSeries<T>
{
    public static TimeSeries<T> Empty => new();

    private List<TimePoint<T>> _values = [];
    public List<TimePoint<T>> Values { get => _values; set => _values = [.. value.OrderBy(t => t.Timestamp)]; }
}
