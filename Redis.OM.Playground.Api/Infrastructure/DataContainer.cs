namespace Redis.OM.Playground.Api.Infrastructure;

public readonly struct DataContainer<T>(T Data)
{
    public static readonly DataContainer<T> Empty = new(default!);
    public T Data { get; init; } = Data;
}
