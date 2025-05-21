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
    private readonly IRedisConnectionProvider _provider = provider;

    public WebApplication Configure(WebApplication app) =>
        app
            .Tee(a => a.MapPost("/person", Add))
            .Tee(a => a.MapGet("/person/{id}", GetById))
            .Tee(a => a.MapGet("/person", Get))
            .Tee(a => a.MapGet("/person/search", Search));

    private Task<IResult> Add([FromBody] Person? person) =>
        person.ToOption()
            .MapAsync(p => p!.Map(pp => _provider.RedisCollection<Person>().InsertAsync(pp, WhenKey.NotExists)))
            .MatchAsync(r => r.ToEither(Results.Conflict()).Match(Results.Ok, c => c), () => Results.StatusCode(StatusCodes.Status400BadRequest));

    private Task<IResult> GetById([FromRoute] Guid id) =>
        _provider
            .RedisCollection<Person>(1)
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync()
            .ToOptionAsync()
            .MatchAsync(r => r.ToEither(Results.StatusCode(StatusCodes.Status400BadRequest))
                .Match(Results.Ok, c => c), () => Results.NotFound());

    private Task<IResult> Get([FromQuery] string? firstName, [FromQuery] string? lastName) =>
        CreateOptionalPredicate(firstName, lastName)
            .Map(p => _provider.RedisCollection<Person>().Where(p))
            .MapAsync(c => c.ToListAsync())
            .MapAsync(l => new DataContainer<IList<Person>>(l))
            .MatchAsync(Results.Ok, () => Results.StatusCode(StatusCodes.Status400BadRequest));

    private static Option<Expression<Func<Person, bool>>> CreateOptionalPredicate(string? firstName, string? lastName) =>
        (firstName, lastName)
            .ToOption(t => t.firstName is null && t.lastName is null)
            .Map<Expression<Func<Person, bool>>>(t => (t.firstName, t.lastName) switch
            {
                (not null, not null) => p => p.FirstName == t.firstName && p.LastName == t.lastName,
                (not null, null) => p => p.FirstName == t.firstName,
                (null, not null) => p => p.LastName == t.lastName,
            });

    private Task<IResult> Search([FromQuery] string? q) =>
        q.ToOption(string.IsNullOrWhiteSpace)
            .Map(q => _provider.RedisCollection<Person>().Raw(q!))
            .MapAsync(c => c.ToListAsync())
            .MapAsync(l => new DataContainer<IList<Person>>(l))
            .MatchAsync(Results.Ok, () => Results.StatusCode(StatusCodes.Status400BadRequest));
}
