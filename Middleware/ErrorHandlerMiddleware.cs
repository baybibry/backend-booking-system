using System.Net;
using System.Text.Json;
using BookingSystem.Enums;
using BookingSystem.Wrapper;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Middleware;

public class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);

            if (!context.Response.HasStarted)
            {
                await HandleStatusCodeAsync(context);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "The resource was modified by another request. Please retry."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        await WriteResponseAsync(context, statusCode, message);
    }

    private static async Task HandleStatusCodeAsync(HttpContext context)
    {
        var message = context.Response.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => "Authentication is required to access this resource.",
            StatusCodes.Status403Forbidden => "You do not have permission to access this resource.",
            StatusCodes.Status404NotFound => "The requested resource was not found.",
            StatusCodes.Status405MethodNotAllowed => "HTTP method not allowed on this endpoint.",
            _ => null
        };

        if (message is null) return;

        await WriteResponseAsync(context, (HttpStatusCode)context.Response.StatusCode, message);
    }

    private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        var statusType = (int)statusCode >= 400 ? StatusType.Error : StatusType.Success;

        var wrapper = ResponseWrapper<object>.On(
            null,
            message,
            statusType,
            (int)statusCode
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(wrapper, JsonOptions));
    }
}
