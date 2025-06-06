using Redis.OM.Playground.AppHost.RedisOs;
using TinyFp.Extensions;

const string ApiName = "api";
const string RedisName = "RedisOs";
const int RedisPort = 6379;

await DistributedApplication
    .CreateBuilder()
    .Map(b => (builder: b, redis: b.AddRedisOs(RedisName, port: RedisPort).WithRedisInsight()))
    .Map(t =>
        t.builder
            .Tee(b =>
                b.AddProject<Projects.Redis_OM_Playground_Api>(ApiName)
                    .WithEnvironment($"ConnectionStrings__{RedisName}", $"localhost:{RedisPort.ToString()}")
                    .WithExternalHttpEndpoints()
                    .WithReference(t.redis)
                    .WaitFor(t.redis)))
    .Build()
    .RunAsync();