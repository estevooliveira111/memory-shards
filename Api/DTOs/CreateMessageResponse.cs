namespace Api.DTOs;

/// <summary>Response body for POST /api/messages.</summary>
public sealed class CreateMessageResponse
{
    /// <summary>The short slug identifier (e.g. "abxqtp").</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Full shareable URL (e.g. "https://domain.com/m/abxqtp").</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the message will expire.</summary>
    public DateTime ExpiresAt { get; set; }
}
