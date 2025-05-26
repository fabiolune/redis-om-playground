namespace Redis.OM.Playground.Api.Configuration.Routing;

public record RoutingConfiguration
{
    public static readonly RoutingConfiguration Default = new()
    {
        PathBase = string.Empty
    };

    public string PathBase { get; init; } = string.Empty;
}
