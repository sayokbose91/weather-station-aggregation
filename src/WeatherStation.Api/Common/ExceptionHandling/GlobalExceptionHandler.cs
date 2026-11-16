using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WeatherStation.Api.Common.ExceptionHandling;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Client disconnected before the request finished");
            return true;
        }

        var status = StatusCodes.Status500InternalServerError;
        var title = "An unexpected error occurred.";
        string? detail = null;

        if (exception is BadHttpRequestException badRequest)
        {
            status = badRequest.StatusCode;
            title = "Bad request";
            detail = badRequest.Message;
            logger.LogWarning("Request rejected with {StatusCode}: {Reason}", status, detail);
        }
        else
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail },
        });
    }
}
