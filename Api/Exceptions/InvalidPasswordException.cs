namespace Api.Exceptions;

/// <summary>
/// Thrown when the provided password cannot decrypt the message.
/// Maps to HTTP 401 Unauthorized.
/// The message is intentionally vague to avoid leaking information.
/// </summary>
public sealed class InvalidPasswordException()
    : Exception("Unauthorized.");
