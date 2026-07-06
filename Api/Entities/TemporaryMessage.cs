namespace Api.Entities;

public sealed class TemporaryMessage
{
    public long Id { get; set; }

    /// <summary>Unique short identifier used in the share URL (only a-z).</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Message content. When <see cref="IsEncrypted"/> is true, this field holds the
    /// AES-256-CBC ciphertext encoded in Base64.
    /// </summary>
    public string EncryptedContent { get; set; } = string.Empty;

    /// <summary>Indicates whether the message is encrypted with a user-supplied password.</summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Base64-encoded random salt (32 bytes) used in PBKDF2 key derivation.
    /// Null when the message is not encrypted.
    /// </summary>
    public string? Salt { get; set; }

    /// <summary>
    /// Base64-encoded AES Initialization Vector (16 bytes).
    /// Null when the message is not encrypted.
    /// </summary>
    public string? IV { get; set; }

    /// <summary>UTC timestamp after which the message can no longer be accessed.</summary>
    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>Returns true if the message has passed its expiration time.</summary>
    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
}
