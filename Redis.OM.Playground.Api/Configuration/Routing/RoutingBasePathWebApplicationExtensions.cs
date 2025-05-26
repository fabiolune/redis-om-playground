using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Configuration.Routing;

public static class RoutingBasePathWebApplicationExtensions
{
    private const char Slash = '/';

    public static WebApplication UseRoutingBasePath(this WebApplication app) =>
        app.Tee(a =>
            (a.Services, App: a)
                .Map(t => (Config: t.Services.GetRequiredService<RoutingConfiguration>(), t.App))
                .ToOption(_ => !_.Config.PathBase.StartsWith(Slash))
                .Map(_ => (_.Config.PathBase, _.App))
                .OnSome(_ => _.App.UsePathBase(_.PathBase).UseRouting()));
}