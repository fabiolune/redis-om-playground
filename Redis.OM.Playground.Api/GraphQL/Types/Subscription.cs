using HotChocolate.Subscriptions;
using Redis.OM.Playground.Api.GraphQL.Resolvers;
using Redis.OM.Playground.Api.Infrastructure;

namespace Redis.OM.Playground.Api.GraphQL.Types;

public class Subscription
{
    public async IAsyncEnumerable<TimeSeries<double>> OnDataChanged(
        [Service] ITimeSeriesResolver resolver,
        [Service] ITopicEventReceiver receiver,
        [Argument] AggregationType aggregation)
    {

        yield return await resolver.ResolveAsync(aggregation);

        var channel = aggregation switch
        {
            AggregationType.Raw => Constants.Channels.Raw,
            AggregationType.OneMinute => Constants.Channels.OneMinute,
            AggregationType.FiveMinutes => Constants.Channels.FiveMinutes,
            AggregationType.FifteenMinutes => Constants.Channels.FifteenMinutes,
            AggregationType.OneHour => Constants.Channels.OneHour,
            _ => throw new NotImplementedException()
        };

        var sourceStream = await receiver.SubscribeAsync<ScheduledUpdate>(channel);

        await foreach (var _ in sourceStream.ReadEventsAsync())
        {
            yield return await resolver.ResolveAsync(aggregation);
        }
    }

    [Subscribe(With = nameof(OnDataChanged))]
    public TimeSeries<double> GetUserCreated([EventMessage] TimeSeries<double> userCreated) => userCreated;
}
