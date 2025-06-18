using NRedisStack;
using NRedisStack.DataTypes;
using NRedisStack.Literals.Enums;
using NRedisStack.RedisStackCommands;
using Redis.OM.Playground.Api.Configuration.Redis;
using Redis.OM.Playground.Api.Messaging;
using TinyFp;
using TinyFp.Extensions;
using static TinyFp.Prelude;

namespace Redis.OM.Playground.Api.HostedServices;

public class TimeSeriesInitializerService(IDatabaseProvider databaseProvider) : IHostedService
{
    private readonly IDatabaseProvider _databaseProvider = databaseProvider;
    private static readonly long RetentionTime = (long)TimeSpan.FromDays(7).TotalMilliseconds;

    private TimeSeriesCommands Ts() => _databaseProvider.GetDatabase().TS();
    private static readonly TsCreateParams OneWeekRetentionParameters = new TsCreateParamsBuilder()
        .AddRetentionTime(RetentionTime)
        .AddDuplicatePolicy(TsDuplicatePolicy.SUM)
        .build();

    public Task StartAsync(CancellationToken cancellationToken) =>
        TryAsync(() => Ts().CreateAsync(MessagingConstants.UserCreatedKey, OneWeekRetentionParameters).ToTaskUnit<Unit>()).ToEither()
            .BindAsync(_ => TryAsync(() => Ts().CreateAsync(MessagingConstants.UserCreated1MinKey, OneWeekRetentionParameters).ToTaskUnit<Unit>()).ToEither())
            .BindAsync(_ => TryAsync(() => Ts().CreateAsync(MessagingConstants.UserCreated5MinKey, OneWeekRetentionParameters).ToTaskUnit<Unit>()).ToEither())
            .BindAsync(_ => TryAsync(() => Ts().CreateAsync(MessagingConstants.UserCreated15MinKey, OneWeekRetentionParameters).ToTaskUnit<Unit>()).ToEither())
            .BindAsync(_ => TryAsync(() => Ts().CreateAsync(MessagingConstants.UserCreated1HourKey, OneWeekRetentionParameters).ToTaskUnit<Unit>()).ToEither())
            .BindAsync(_ => TryAsync(() => Ts().CreateRuleAsync(MessagingConstants.UserCreatedKey, new TimeSeriesRule(MessagingConstants.UserCreated1MinKey, 60_000, NRedisStack.Literals.Enums.TsAggregation.Sum)).ToTaskUnit<Unit>()).ToEither())
            .BindAsync(_ => TryAsync(() => Ts().CreateRuleAsync(MessagingConstants.UserCreatedKey, new TimeSeriesRule(MessagingConstants.UserCreated5MinKey, 300_000, NRedisStack.Literals.Enums.TsAggregation.Sum)).ToTaskUnit<Unit>()).ToEither())
            .BindAsync(_ => TryAsync(() => Ts().CreateRuleAsync(MessagingConstants.UserCreatedKey, new TimeSeriesRule(MessagingConstants.UserCreated15MinKey, 900_000, NRedisStack.Literals.Enums.TsAggregation.Sum)).ToTaskUnit<Unit>()).ToEither())
            .BindAsync(_ => TryAsync(() => Ts().CreateRuleAsync(MessagingConstants.UserCreatedKey, new TimeSeriesRule(MessagingConstants.UserCreated1HourKey, 3_600_000, NRedisStack.Literals.Enums.TsAggregation.Sum)).ToTaskUnit<Unit>()).ToEither())
            .ToTaskUnit<Unit>();

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
