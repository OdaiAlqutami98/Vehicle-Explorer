using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VehicleExplorer.Application.Exceptions;

namespace VehicleExplorer.Api.ErrorHandling;

internal sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            InvalidRequestException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                exception.Message),

            ExternalServiceException => (
                StatusCodes.Status502BadGateway,
                "Vehicle data source unavailable",
                "NHTSA vPIC did not respond correctly. Please try again later."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "Something went wrong on our side."),
        };

        if (statusCode == StatusCodes.Status502BadGateway)
        {
            logger.LogWarning(exception, "Vehicle data source failed");
        }
        else if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
            },
        });
    }
}
