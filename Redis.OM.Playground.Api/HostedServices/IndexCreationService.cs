using Redis.OM.Contracts;
using Redis.OM.Playground.Api.Modelling;
using TinyFp;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.HostedServices;

public class IndexCreationService(IRedisConnectionProvider provider) : IHostedService
{
    private readonly IRedisConnectionProvider _provider = provider;
    private static readonly Type PersonType = typeof(Person);

    //=> _provider.Connection.CreateIndexAsync(typeof(Person));
    public Task StartAsync(CancellationToken cancellationToken) => 
        _provider
            .Connection
            .GetIndexInfoAsync(PersonType)
            .ToOptionAsync()
            .MapAsync(i => i!)
            .MatchAsync(i => _provider.Connection.IsIndexCurrentAsync(PersonType).ToOptionAsync(b => b).MapAsync(_ => i), Option<RedisIndexInfo>.None)
            .MatchAsync(i => _provider.Connection.DropIndexAsync(PersonType).ToOptionAsync(), Option<bool>.None)
            .MatchAsync(_ => Unit.Default, () => Unit.Default)
            .MapAsync(_ => _provider.Connection.CreateIndexAsync(PersonType));

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
