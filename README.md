# Redis.OM.Playground

This project demonstrates how to use Redis OM (Object Mapping) for .NET, showcasing integration with Redis Stack modules (RedisJSON, RediSearch) through a minimal API.

## Overview

Redis OM for .NET provides high-level abstractions for working with Redis data structures through C# objects and LINQ-like queries. This playground demonstrates:

- Document storage using RedisJSON
- Indexing and searching with RediSearch
- Functional programming patterns with TinyFp
- ASP.NET Core minimal API endpoints

## Project Structure

- **Redis.OM.Playground.Api**: Core application with endpoints and models
- **manifests/**: sample Kubernetes manifests for deploying the application
- **run.sh**: Script to set up the local test environment and run integration tests

## Endpoints

| Method | Path                           | Description                    |
| ------ | ------------------------------ | ------------------------------ |
| POST   | `/person`                      | Create a new person document   |
| GET    | `/person/{id}`                 | Retrieve a person by ID        |
| GET    | `/person?firstName=&lastName=` | Find people by first/last name |
| GET    | `/person/search?q=`            | Perform raw Redis search       |

## Key Features

### Model Definition

```csharp
[Document(StorageType = StorageType.Json, Prefixes = ["person"])]
public record Person
{
    [RedisIdField]
    [Indexed]
    public Guid Id { get; init; } = Guid.CreateVersion7();

    [Searchable]
    public string? FirstName { get; set; }

    [Searchable]
    public string? LastName { get; set; }

    [Searchable(Sortable = true)]
    public string? PersonalStatement { get; set; }

    # ... additional code
}
```

### Functional Programming Approach

The application uses the TinyFp library to implement functional programming patterns:

```csharp
private Task<IResult> GetById([FromRoute] Guid id) =>
    _provider
        .RedisCollection<Person>(1)
        .Where(p => p.Id == id)
        .FirstOrDefaultAsync()
        .ToOptionAsync()
        .MatchAsync(Results.Ok, () => Results.NotFound());
```

> example of using TinyFp with minimal API

### Redis OM Features

- **Collections**: `RedisCollection<T>` for type-safe access
- **Querying**: LINQ-style querying against Redis indexes
- **Raw Queries**: Direct RediSearch queries for advanced use cases

## Running the Project

### `dotnet `Aspire

The solution uses dotnet Aspire to orchestrate the development and spin up the proper dependencies.
From a `windows` ou can run the application by executing

```shell
dotnet run --project .\Redis.OM.Playground.AppHost\Redis.OM.Playground.AppHost.csproj
```

The same result can be obtained opening the [redis-om-playground.sln](redis-om-playground.sln) solution with VisualStudio and launching the `Redis.OM.Playground.AppHost` project.

### Integration tests

Prerequisites:

- Docker
- kubectl
- Helm
- Kind (Kubernetes in Docker)

1. Run the setup script:

```bash
chmod +x *.sh
./run.sh
```

This will:

- Create a local Kind Kubernetes cluster
- Set up a Redis Cluster with required modules
- Build and deploy the application
- Execute integration tests to verify functionality

The integration tests in [run.sh](run.sh) verify:

- Creation of person records
- Retrieval of person records by ID
- Search functionality

#### Clean Up

To clean up the integration testing environment:

```bash
./stop.sh
```

## Technology Stack

- .NET 9.0
- Redis OS (Redis + RedisJSON + RediSearch)
- Redis OM for .NET
- TinyFp[^1] (Functional programming library)
- Docker & Kubernetes

## License

MIT License

[^1]: https://github.com/FrancoMelandri/tiny-fp
