
using HotChocolate.Subscriptions;
using Redis.OM.Playground.Api.GraphQL;
using TinyFp;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.HostedServices;

public class TimeTriggerService(TimeProvider provider, ITopicEventSender eventSender) : IHostedService
{
    private readonly TimeProvider _provider = provider;
    private readonly ITopicEventSender _eventSender = eventSender;

    public Task StartAsync(CancellationToken cancellationToken) =>
        _provider
            .Tee(p => p.CreateTimer(async _ => await _eventSender.SendAsync(Constants.Channels.Raw, ScheduledUpdate.New()), this, TimeSpan.Zero, TimeSpan.FromSeconds(4)))
            .Tee(p => p.CreateTimer(async _ => await _eventSender.SendAsync(Constants.Channels.OneMinute, ScheduledUpdate.New()), this, TimeSpan.Zero, TimeSpan.FromSeconds(12)))
            .Tee(p => p.CreateTimer(async _ => await _eventSender.SendAsync(Constants.Channels.FiveMinutes, ScheduledUpdate.New()), this, TimeSpan.Zero, TimeSpan.FromMinutes(1)))
            .Tee(p => p.CreateTimer(async _ => await _eventSender.SendAsync(Constants.Channels.FifteenMinutes, ScheduledUpdate.New()), this, TimeSpan.Zero, TimeSpan.FromMinutes(3)))
            .Tee(p => p.CreateTimer(async _ => await _eventSender.SendAsync(Constants.Channels.OneHour, ScheduledUpdate.New()), this, TimeSpan.Zero, TimeSpan.FromMinutes(12)))
            .AsTask();

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
