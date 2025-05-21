using Redis.OM.Modeling;

namespace Redis.OM.Playground.Api.Modelling;

[Document(StorageType = StorageType.Json, Prefixes = ["person"])]
public record Person
{
    [RedisIdField]
    [Indexed]
    public Guid Id { get; init; } = Guid.CreateVersion7();

    [Indexed] public string? FirstName { get; set; }

    [Indexed] public string? LastName { get; set; }

    [Indexed] public int Age { get; set; }

    [Searchable] public string? PersonalStatement { get; set; }

    [Indexed] public string[] Skills { get; set; } = [];

    [Indexed(CascadeDepth = 1)] public Address? Address { get; set; }
}
