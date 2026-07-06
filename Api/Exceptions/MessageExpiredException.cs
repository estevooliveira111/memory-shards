namespace Api.Exceptions;

/// <summary>
/// Thrown when a message exists but has passed its expiration date.
/// Maps to HTTP 410 Gone.
/// </summary>
public sealed class MessageExpiredException(string slug)
    : Exception($"Message with slug '{slug}' has expired.");
