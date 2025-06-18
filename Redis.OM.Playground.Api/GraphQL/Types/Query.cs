using Redis.OM.Playground.Api.GraphQL.Resolvers;
using Redis.OM.Playground.Api.Infrastructure;

namespace Redis.OM.Playground.Api.GraphQL.Types;

public class Query(ITimeSeriesResolver resolver)
{
    private readonly ITimeSeriesResolver _resolver = resolver;
    public Task<TimeSeries<double>> GetUserCreated([Argument] AggregationType aggregation) =>
        _resolver.ResolveAsync(aggregation);
}