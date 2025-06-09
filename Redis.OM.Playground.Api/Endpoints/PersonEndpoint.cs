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
    private const int MaxPageSize = 100;
    private readonly IRedisConnectionProvider _provider = provider;

    public WebApplication Configure(WebApplication app) =>
        app
            .Tee(a => a.MapPost("/person", Add))
            .Tee(a => a.MapGet("/person/{id}", GetById))
            .Tee(a => a.MapGet("/person", Get))
            .Tee(a => a.MapGet("/person/search", Search))
            .Tee(a => a.MapGet("/person/list", List))
            .Tee(a => a.MapGet("/person/count", Count));

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
            .MatchAsync(Results.Ok, () => Results.NotFound());

    private Task<IResult> Get([FromQuery] string? firstName, [FromQuery] string? lastName) =>
        CreateOptionalPredicate(firstName, lastName)
            .Map(p => _provider.RedisCollection<Person>().Where(p))
            .MapAsync(c => c.ToListAsync())
            .MapAsync(l => new DataContainer<IList<Person>>(l))
            .MatchAsync(Results.Ok, () => Results.StatusCode(StatusCodes.Status400BadRequest));

    #pragma warning disable CS8655 // The switch expression does not need to handle the (null, null) case because the 'ToOption' above is already excluding it
    private static Option<Expression<Func<Person, bool>>> CreateOptionalPredicate(string? firstName, string? lastName) =>
        (firstName, lastName)
            .ToOption(t => t.firstName is null && t.lastName is null)
            .Map<Expression<Func<Person, bool>>>(t => (t.firstName, t.lastName) switch
            {
                (not null, not null) => p => p.FirstName == t.firstName && p.LastName == t.lastName,
                (not null, null) => p => p.FirstName == t.firstName,
                (null, not null) => p => p.LastName == t.lastName,
            });
#pragma warning restore CS8655 // The switch expression does not handle some null inputs.

    private Task<IResult> Search([FromQuery(Name = "q")] string? query) =>
        query.ToOption(string.IsNullOrWhiteSpace)
            .Map(q => _provider.RedisCollection<Person>().Raw(q!))
            .MapAsync(c => c.ToListAsync())
            .MapAsync(l => new DataContainer<IList<Person>>(l))
            .MatchAsync(Results.Ok, () => Results.StatusCode(StatusCodes.Status400BadRequest));

    private Task<IResult> List([FromQuery(Name = "p")] int page = 1, [FromQuery(Name = "s")] int limit = 10) =>
        (page, limit)
            .ToOption(t => t.page <= 0 || t.limit <= 0 || limit > MaxPageSize)
            .Map(t => _provider.RedisCollection<Person>(t.limit).Skip((page - 1) * limit).Take(limit))
            .MapAsync(c => c.ToListAsync())
            .MapAsync(l => new DataContainer<IList<Person>>(l))
            .MatchAsync(Results.Ok, () => Results.StatusCode(StatusCodes.Status400BadRequest));

    private Task<IResult> Count() =>
        _provider
            .RedisCollection<Person>()
            .CountAsync()
            .MapAsync(c => Task.FromResult(Results.Ok(new DataContainer<int>(c))));
}
