using Redis.OM.Playground.Api.Infrastructure;

namespace Redis.OM.Playground.Api.GraphQL.Resolvers;

public interface ITimeSeriesResolver
{
    public Task<TimeSeries<double>> ResolveAsync(AggregationType aggregation);
}
