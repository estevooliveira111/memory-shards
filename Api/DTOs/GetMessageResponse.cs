namespace Api.DTOs;

/// <summary>Response body for GET /api/messages/{slug}.</summary>
public sealed class GetMessageResponse
{
    /// <summary>The plaintext message content.</summary>
    public string Content { get; set; } = string.Empty;
}
