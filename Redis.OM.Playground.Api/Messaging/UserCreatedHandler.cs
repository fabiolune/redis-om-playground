using Mediator;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using Redis.OM.Playground.Api.Configuration.Redis;
using StackExchange.Redis;
using TinyFp;
using TinyFp.Extensions;
using Unit = TinyFp.Unit;

namespace Redis.OM.Playground.Api.Messaging;

public sealed class UserCreatedHandler(IDatabaseProvider provider) : IRequestHandler<UserCreated, Unit>
{
    private readonly IDatabase _db = provider.GetDatabase();

    public ValueTask<Unit> Handle(UserCreated request, CancellationToken cancellationToken) =>
        _db.TS()
            .IncrByAsync(MessagingConstants.UserCreatedKey, 1)
            //.AddAsync(MessagingConstants.UserCreatedKey, new TsAddParamsBuilder().AddTimestamp(DateTime.UtcNow).AddValue(1).build())
            .ToTaskUnit<Unit>()
            .Map(t => new ValueTask<Unit>(t));
}
