using Redis.OM.Contracts;
using Redis.OM.Playground.Api.Modelling;
using TinyFp;
using TinyFp.Extensions;
using static TinyFp.Prelude;

namespace Redis.OM.Playground.Api.HostedServices;

public class IndexCreationService(IRedisConnectionProvider provider) : IHostedService
{
    private readonly IRedisConnection _connection = provider.Connection;
    private static readonly Type PersonType = typeof(Person);

    private Task<Unit> CreateIndex() => _connection.CreateIndexAsync(PersonType).ToTaskUnit<bool>();

    private Task<Unit> DropAndCreateIndex() =>
        _connection
            .DropIndexAsync(PersonType)
            .MapAsync(_ => _connection.CreateIndexAsync(PersonType))
            .ToTaskUnit<bool>();

    public Task StartAsync(CancellationToken cancellationToken) =>
        TryAsync(() => _connection.GetIndexInfoAsync(PersonType))
            .Match(
                info => _connection.IsIndexCurrentAsync(PersonType).ToOptionAsync(b => !b)
                    .MatchAsync(_ => Unit.Default, () => DropAndCreateIndex()),
                _ => CreateIndex());

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
