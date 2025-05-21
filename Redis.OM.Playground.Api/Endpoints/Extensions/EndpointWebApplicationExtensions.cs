using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Endpoints.Extensions;

public static class EndpointWebApplicationExtensions
{
    public static WebApplication UseEndpointDefinitions(this WebApplication app) =>
        app.Tee(a => a
            .Services
            .GetRequiredService<IEnumerable<IEndpoint>>()
            .ForEach(e => e.Configure(app)));
}