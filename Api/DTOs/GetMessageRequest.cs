namespace Api.DTOs;

/// <summary>Optional request body for GET /api/messages/{slug}.</summary>
public sealed class GetMessageRequest
{
    /// <summary>Password required to decrypt the message. Only needed for protected messages.</summary>
    public string? Password { get; set; }
}
