namespace Api.Exceptions;

/// <summary>
/// Thrown when a message with the requested slug does not exist in the database.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class MessageNotFoundException(string slug)
    : Exception($"Message with slug '{slug}' was not found.");
