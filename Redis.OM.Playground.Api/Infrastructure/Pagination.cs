namespace Redis.OM.Playground.Api.Infrastructure;

public readonly struct Pagination
{
    public int Page { get; init; }
    public int Total { get; init; }
    public int Limit { init; get; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
}
