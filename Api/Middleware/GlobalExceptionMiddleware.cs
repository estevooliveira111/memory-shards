using System.Net;
using System.Text.Json;
using Api.Exceptions;
using FluentValidation;

namespace Api.Middleware;

/// <summary>
/// Catches domain exceptions and converts them to structured JSON responses.
/// Ensures no internal details are leaked to the client in production.
/// </summary>
public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.UnprocessableEntity, new
            {
                title  = "Validation failed.",
                errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }
        catch (MessageNotFoundException ex)
        {
            logger.LogInformation(ex, "Message not found.");
            await WriteResponseAsync(context, HttpStatusCode.NotFound, new
            {
                title = "Message not found."
            });
        }
        catch (MessageExpiredException ex)
        {
            logger.LogInformation(ex, "Message expired.");
            await WriteResponseAsync(context, HttpStatusCode.Gone, new
            {
                title = "Message has expired."
            });
        }
        catch (InvalidPasswordException)
        {
            // Intentionally vague — do not reveal the reason
            await WriteResponseAsync(context, HttpStatusCode.Unauthorized, new
            {
                title = "Unauthorized."
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError, new
            {
                title = "An internal error occurred."
            });
        }
    }

    private static Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, object body)
    {
        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
