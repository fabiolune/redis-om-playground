namespace Redis.OM.Playground.Api.Infrastructure;

public readonly struct DataContainer<T>(T data)
{
    public T Data { get; init; } = data;
}
