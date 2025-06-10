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
                    .WithExternalHttpEndpoints()
                    .WithReference(t.redis)
                    .WaitFor(t.redis)))
    .Map(t => t.builder
            .Tee(b => b.AddNpmApp("ui", "../redis.om.playground.ui", "dev")
                .WithHttpEndpoint(env: "PORT")
                .WithExternalHttpEndpoints()
                .WaitFor(t.api)
                .WithEnvironment("API_BASE_URL", t.api.GetEndpoint("http"))
                .WithEnvironment("NODE_ENV", "development")
                ))
    .Build()
    .RunAsync();