using Redis.OM.Modeling;

namespace Redis.OM.Playground.Api.Modelling;

public record Address
{
    [Indexed]
    public int? StreetNumber { get; set; }

    [Indexed]
    public string? Unit { get; set; }

    [Searchable]
    public string? StreetName { get; set; }

    [Indexed]
    public string? City { get; set; }

    [Indexed]
    public string? State { get; set; }

    [Indexed]
    public string? PostalCode { get; set; }

    [Indexed]
    public string? Country { get; set; }

    [Indexed]
    public GeoLoc Location { get; set; }
}
