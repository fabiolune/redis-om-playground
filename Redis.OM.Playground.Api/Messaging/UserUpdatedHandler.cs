using Mediator;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using Redis.OM.Playground.Api.Configuration.Redis;
using StackExchange.Redis;
using TinyFp;
using TinyFp.Extensions;
using Unit = TinyFp.Unit;

namespace Redis.OM.Playground.Api.Messaging;

public sealed class UserUpdatedHandler(IDatabaseProvider provider) : IRequestHandler<UserUpdated, Unit>
{
    private readonly IDatabase _db = provider.GetDatabase();

    public ValueTask<Unit> Handle(UserUpdated request, CancellationToken cancellationToken) =>
        _db.TS()
            .AddAsync(MessagingConstants.UserUpdatedKey, new TsAddParamsBuilder().AddTimestamp(DateTime.UtcNow).AddValue(1).build())
            .ToTaskUnit<Unit>()
            .Map(t => new ValueTask<Unit>(t));
}