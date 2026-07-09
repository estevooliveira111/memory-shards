using System.Text.Json;
using Api.Exceptions;
using Api.Middleware;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private static async Task<(int StatusCode, JsonDocument Body)> InvokeAsync(Exception exceptionToThrow)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionMiddleware(
            _ => throw exceptionToThrow,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, JsonDocument.Parse(json));
    }

    [Fact]
    public async Task ValidationException_Returns422WithFieldErrors()
    {
        var failures = new[] { new ValidationFailure("Content", "O conteúdo é obrigatório.") };
        var (statusCode, body) = await InvokeAsync(new FluentValidation.ValidationException(failures));

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, statusCode);
        Assert.Equal("Validation failed.", body.RootElement.GetProperty("title").GetString());

        var error = body.RootElement.GetProperty("errors")[0];
        Assert.Equal("Content", error.GetProperty("field").GetString());
        Assert.Equal("O conteúdo é obrigatório.", error.GetProperty("message").GetString());
    }

    [Fact]
    public async Task MessageNotFoundException_Returns404()
    {
        var (statusCode, body) = await InvokeAsync(new MessageNotFoundException("abc123"));

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.Equal("Message not found.", body.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task MessageExpiredException_Returns410()
    {
        var (statusCode, body) = await InvokeAsync(new MessageExpiredException("abc123"));

        Assert.Equal(StatusCodes.Status410Gone, statusCode);
        Assert.Equal("Message has expired.", body.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task InvalidPasswordException_Returns401WithVagueMessage()
    {
        var (statusCode, body) = await InvokeAsync(new InvalidPasswordException());

        Assert.Equal(StatusCodes.Status401Unauthorized, statusCode);
        Assert.Equal("Unauthorized.", body.RootElement.GetProperty("title").GetString());

        // Must not leak any internal detail beyond the generic message
        Assert.False(body.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task UnhandledException_Returns500WithGenericMessage()
    {
        var (statusCode, body) = await InvokeAsync(new InvalidOperationException("some internal detail"));

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.Equal("An internal error occurred.", body.RootElement.GetProperty("title").GetString());

        // The internal exception message must never be exposed to the client
        var raw = body.RootElement.ToString();
        Assert.DoesNotContain("some internal detail", raw);
    }

    [Fact]
    public async Task Response_ContentTypeIsJson()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionMiddleware(
            _ => throw new MessageNotFoundException("abc"),
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal("application/json", context.Response.ContentType);
    }
}
