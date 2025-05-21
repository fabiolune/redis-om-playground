using Redis.OM.Contracts;
using Redis.OM.Playground.Api.Modelling;

namespace Redis.OM.Playground.Api.HostedServices;

public class IndexCreationService(IRedisConnectionProvider provider) : IHostedService
{
    private readonly IRedisConnectionProvider _provider = provider;

    public Task StartAsync(CancellationToken cancellationToken) => _provider.Connection.CreateIndexAsync(typeof(Person));

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
