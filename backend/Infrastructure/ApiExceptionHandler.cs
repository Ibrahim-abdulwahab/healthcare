using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Healthcare.Api.Infrastructure;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            ApiException e => e.Status,
            ArgumentException => 400,
            DbUpdateConcurrencyException => 409,
            DbUpdateException { InnerException: PostgresException { SqlState: "23505" or "23503" or "40001" or "40P01" } } => 409,
            PostgresException { SqlState: "40001" or "40P01" } => 409,
            _ => 500
        };
        if (status == 500) logger.LogError("Request {TraceId} failed with {ExceptionType}.", context.TraceIdentifier, exception.GetType().Name);
        var detail = exception is ApiException or ArgumentException ? exception.Message : status == 409
            ? "This change conflicts with existing or recently changed records. Refresh and try again."
            : "The request could not be completed. Please try again.";
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = status == 409 ? "Conflict" : "Request failed", Detail = detail, Extensions = { ["traceId"] = context.TraceIdentifier } }, cancellationToken);
        return true;
    }
}
