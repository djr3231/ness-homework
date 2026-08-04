using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Gaia.Api.Errors;

public sealed class UnexpectedExceptionHandler : IExceptionHandler
{
    private readonly ILogger<UnexpectedExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public UnexpectedExceptionHandler(
        ILogger<UnexpectedExceptionHandler> logger,
        IProblemDetailsService problemDetailsService
    )
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(
            exception,
            "An unexpected exception occurred while processing the request."
        );

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Unexpected server error",
            Detail = "An unexpected error occurred while processing the request.",
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
        };

        return _problemDetailsService.TryWriteAsync(problemDetailsContext);
    }
}
