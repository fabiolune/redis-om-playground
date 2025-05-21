using Microsoft.AspNetCore.Mvc;
using Redis.OM.Contracts;
using Redis.OM.Playground.Api.Infrastructure;
using Redis.OM.Playground.Api.Modelling;
using System.Linq.Expressions;
using TinyFp;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Endpoints;

public class PersonEndpoint(IRedisConnectionProvider provider) : IEndpoint
{
    public WebApplication Configure(WebApplication app) =>
        app
            .Tee(a => a.MapPost("/person", Add))
            .Tee(a => a.MapGet("/person/{id}", GetById))
            .Tee(a => a.MapGet("/person", Get))
            .Tee(a => a.MapGet("/person/search", Search));

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
            .Where(CreatePredicate(firstName, lastName))
            .ToListAsync()
            .ToOptionAsync()
            .MatchAsync(
                r => r.ToEither(Results.NotFound())
                    .Map(l => new DataContainer<IList<Person>>(l))
                    .Match(Results.Ok, c => c),
                () => Results.StatusCode(StatusCodes.Status400BadRequest));

    private static Expression<Func<Person, bool>> CreatePredicate(string? firstName, string? lastName) =>
        (firstName, lastName) switch
        {
            (not null, not null) => p => p.FirstName == firstName && p.LastName == lastName,
            (not null, null) => p => p.FirstName == firstName,
            (null, not null) => p => p.LastName == lastName,
            _ => p => p.FirstName != null // dynamic way to express a "false" and allow the code to properly parse the expression
        };

    private Task<IResult> Search([FromQuery] string? q) =>
        q.ToOption(string.IsNullOrWhiteSpace)
            .Map(q => q!).AsTask()
            .MatchAsync(q => provider.RedisCollection<Person>().Raw(q).ToListAsync().ToOptionAsync(), Option<IList<Person>>.None)
            .MapAsync(l => new DataContainer<IList<Person>>(l))
            .MatchAsync(d => Results.Ok(d), () => Results.StatusCode(StatusCodes.Status400BadRequest));
}
