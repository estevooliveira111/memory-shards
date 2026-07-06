namespace Api.DTOs;

/// <summary>Request body for POST /api/messages.</summary>
public sealed class CreateMessageRequest
{
    /// <summary>Message content. Required. Max 50,000 characters.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Expiration period. Allowed values: "12h", "7d", "1m".</summary>
    public string Expiration { get; set; } = string.Empty;

    /// <summary>Optional numeric password (1–6 digits). When provided, message is encrypted.</summary>
    public string? Password { get; set; }
}
