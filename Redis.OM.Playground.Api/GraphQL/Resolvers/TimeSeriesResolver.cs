using NRedisStack.RedisStackCommands;
using Redis.OM;
using Redis.OM.Playground.Api.Configuration.Redis;
using Redis.OM.Playground.Api.Infrastructure;
using Redis.OM.Playground.Api.Messaging;
using StackExchange.Redis;
using System.Collections.Immutable;
using TinyFp;
using TinyFp.Extensions;
using static TinyFp.Prelude;

namespace Redis.OM.Playground.Api.GraphQL.Resolvers;

public class TimeSeriesResolver(IDatabaseProvider dbProvider, TimeProvider provider) : ITimeSeriesResolver
{
    private readonly IDatabase _db = dbProvider.GetDatabase();
    private readonly TimeProvider _provider = provider;

    private static readonly IDictionary<AggregationType, string> AggregationTypeToAggregatedSeries = new Dictionary<AggregationType, string>
    {
        { AggregationType.Raw, MessagingConstants.UserCreatedKey },
        { AggregationType.OneMinute, MessagingConstants.UserCreated1MinKey },
        { AggregationType.FiveMinutes, MessagingConstants.UserCreated5MinKey },
        { AggregationType.FifteenMinutes, MessagingConstants.UserCreated15MinKey },
        { AggregationType.OneHour, MessagingConstants.UserCreated1HourKey}
    }.ToImmutableDictionary();

    private static readonly IDictionary<AggregationType, TimeSpan> AggregationTypeToInteval = new Dictionary<AggregationType, TimeSpan>
    {
        { AggregationType.Raw, TimeSpan.FromMinutes(5) },
        { AggregationType.OneMinute, TimeSpan.FromMinutes(10) },
        { AggregationType.FiveMinutes, TimeSpan.FromHours(1) },
        { AggregationType.FifteenMinutes, TimeSpan.FromHours(3) },
        { AggregationType.OneHour, TimeSpan.FromDays(1) }
    }.ToImmutableDictionary();

    private static readonly IDictionary<AggregationType, TimeSpan> AggregationTypeToPrecision = new Dictionary<AggregationType, TimeSpan>
    {
        { AggregationType.Raw, TimeSpan.FromSeconds(10) },
        { AggregationType.OneMinute, TimeSpan.FromMinutes(1) },
        { AggregationType.FiveMinutes, TimeSpan.FromMinutes(5) },
        { AggregationType.FifteenMinutes, TimeSpan.FromMinutes(15) },
        { AggregationType.OneHour, TimeSpan.FromHours(1) }
    }.ToImmutableDictionary();

    private static Dictionary<long, double> GetZeroedTimeSeries(DateTimeOffset start, DateTimeOffset end, TimeSpan precision) =>
        (start, end, precision)
            .Map(t => (t.start, t.end, precision: t.precision.Ticks, steps: (int)((t.end - t.start).TotalMilliseconds / t.precision.TotalMilliseconds)))
            .Map(t => Enumerable.Range(0, t.steps).Select(i => new KeyValuePair<long, double>(start.AddTicks(i * t.precision).ToUnixTimeMilliseconds(), 0)))
            .Map(e => e.ToDictionary());

    private static TimeSeries<double> ToTimeSeries(Dictionary<long, double> data) =>
        data
            .Select(kv => new TimePoint<double>(kv.Key, kv.Value))
            .ToList()
            .Map(l => new TimeSeries<double>
            {
                Values = l
            });

    public Task<TimeSeries<double>> ResolveAsync(AggregationType aggregation) =>
        (now: _provider.GetUtcNow(), precision: AggregationTypeToPrecision[aggregation])
            .Map(t => (end: t.now.AddTicks(t.precision.Ticks - t.now.Ticks % t.precision.Ticks), t.precision))
            .Map(t => (t.end, start: t.end.Subtract(AggregationTypeToInteval[aggregation]), t.precision))
            .Map(t => (t.end, t.start, zeroes: GetZeroedTimeSeries(t.start, t.end, t.precision)))
            .Map(t => TryAsync(() => _db.TS().RangeAsync(AggregationTypeToAggregatedSeries[aggregation], t.start.ToUnixTimeMilliseconds(), t.end.ToUnixTimeMilliseconds()))
                .ToEither()
                .MatchAsync(ts => 
                    ts.Select(t => new KeyValuePair<long, double>(t.Time, t.Val))
                        .ToDictionary()
                        .Map(l => l.Concat(t.zeroes.Where(z => !l.ContainsKey(z.Key))))
                        .ToDictionary().Map(ToTimeSeries), _ => t.zeroes.Map(ToTimeSeries))
            );

}
