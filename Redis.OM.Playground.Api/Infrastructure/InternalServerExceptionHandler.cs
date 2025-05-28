using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using TinyFp.Extensions;

namespace Redis.OM.Playground.Api.Infrastructure;

public class InternalServerExceptionHandler(ILogger<InternalServerExceptionHandler> logger) : IExceptionHandler

{
    private readonly ILogger<InternalServerExceptionHandler> _logger = logger;

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken) =>
        (httpContext, exception, cancellationToken)
            .Tee(t => _logger.LogError(t.exception, "Exception occurred: {Message}", exception.Message))
            .Map(t => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError)
            }.Map(p => t.httpContext.Response.WriteAsJsonAsync(p, t.cancellationToken)))
            .Map(_ => new ValueTask<bool>(true));
}
