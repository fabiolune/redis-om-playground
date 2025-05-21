using Microsoft.AspNetCore.Mvc;
using Redis.OM.Contracts;
using Redis.OM.Playground.Api.Infrastructure;
using Redis.OM.Playground.Api.Modelling;
using TinyFp;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Endpoints;

public class PersonEndpoint(IRedisConnectionProvider provider) : IEndpoint
{
    public WebApplication Configure(WebApplication app) =>
        app
            .Tee(a => a.MapPost("/person", Add))
            .Tee(a => a.MapGet("/person/{id}", GetById))
            .Tee(a => a.MapGet("/person", Get));

    private Task<IResult> Add([FromBody] Person? person) =>
        person.ToOption()
            .MapAsync(p => p!.Map(pp => provider.RedisCollection<Person>().InsertAsync(pp, WhenKey.NotExists)))
            .MatchAsync(r => r.ToEither(Results.Conflict()).Match(Results.Ok, c => c), () => Results.StatusCode(StatusCodes.Status400BadRequest));

    private Task<IResult> GetById([FromRoute] Guid id) =>
        provider
            .RedisCollection<Person>(1)
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync()
            .ToOptionAsync()
            .MatchAsync(r => r.ToEither(Results.StatusCode(StatusCodes.Status400BadRequest))
                .Match(Results.Ok, c => c), () => Results.NotFound());

    private Task<IResult> Get([FromQuery] string? firstName, [FromQuery] string? lastName) =>
        provider.RedisCollection<Person>()
            .Where(p => p.FirstName == firstName && p.LastName == lastName)
            .ToListAsync()
            .ToOptionAsync()
            .MatchAsync(
                r => r.ToEither(Results.NotFound())
                    .Map(l => new DataContainer<IList<Person>>(l))
                    .Match(Results.Ok, c => c), 
                () => Results.StatusCode(StatusCodes.Status400BadRequest));
}
