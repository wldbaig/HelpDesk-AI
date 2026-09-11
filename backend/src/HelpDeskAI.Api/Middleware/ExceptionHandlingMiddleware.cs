using HelpDeskAI.Application.Common;

namespace HelpDeskAI.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
        }
        catch (Exception exception)
        {
            var (status, message) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, exception.Message),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
                ExternalServiceException => (StatusCodes.Status502BadGateway, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };
            logger.LogError(exception, "Request failed with status {StatusCode} for {Method} {Path}", status, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ApiResponse<object>(false, null, message));
        }
    }
}
