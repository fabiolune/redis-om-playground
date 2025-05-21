namespace Redis.OM.Playground.Api.Infrastructure;

public readonly struct DataContainer<T>(T Data)
{
    public T Data { get; init; } = Data;
}
