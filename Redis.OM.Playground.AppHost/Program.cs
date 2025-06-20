using Redis.OM.Playground.AppHost.RedisOs;
using TinyFp.Extensions;

const string ApiName = "api";
const string RedisName = "RedisOs";
const int RedisPort = 6379;

await DistributedApplication
    .CreateBuilder()
    .Map(b => (builder: b, redis: b.AddRedisOs(RedisName, port: RedisPort).WithRedisInsight()))
    .Map(t => (t.builder, api: t.builder.AddProject<Projects.Redis_OM_Playground_Api>(ApiName)
                .WithEnvironment($"ConnectionStrings__{RedisName}", $"localhost:{RedisPort.ToString()}")
                .WithEndpoint(port: 8080, name: "http", scheme: "http")
                .WithEndpoint(port: 8443, name: "https", scheme: "https")
                .WithEndpoint(port: 8080, name: "ws", scheme: "ws", isProxied: false)
                .WithEndpoint(port: 8443, name: "wss", scheme: "wss", isProxied: false)
                .WithExternalHttpEndpoints()
                .WithReference(t.redis)
                .WaitFor(t.redis)))
    .Map(t => t.builder
            .Tee(b => b.AddNpmApp("ui", "../redis.om.playground.ui", "dev")
                .WithHttpEndpoint(env: "PORT")
                .WithExternalHttpEndpoints()
                .WaitFor(t.api)
                .WithEnvironment("API_BASE_URL", t.api.GetEndpoint("http"))
                .WithEnvironment("API_SOCKET_URL", t.api.GetEndpoint("wss"))
                .WithEnvironment("GRAPHQL_PATH", "/graphql")
                .WithEnvironment("NODE_ENV", "development")
                ))
    .Build()
    .RunAsync();