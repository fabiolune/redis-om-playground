namespace Redis.OM.Playground.Api.Infrastructure;

public readonly struct PaginatedDataContainer<T>(IList<T> Data, int Page, int PageSize, int Total)
{
    public IList<T> Data { get; init; } = Data;
    public Pagination Pagination { get; init; } = new Pagination
    {
        Page = Page,
        Total = Total,
        Limit = PageSize
    };
}