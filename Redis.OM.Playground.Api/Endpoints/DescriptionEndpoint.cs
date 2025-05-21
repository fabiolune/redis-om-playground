
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Endpoints;

public class DescriptionEndpoint : IEndpoint
{
    private const string Path = "/internal/description";
    private const string Description = "Redis OM Playground";

    public WebApplication Configure(WebApplication app) =>
        app.Tee(a => a.MapGet(Path, () => Description));
}
