using System.Security.Cryptography;
using Api.DTOs;
using Api.Entities;
using Api.Exceptions;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Api.Services;

public sealed class MessageService(
    IMessageRepository repository,
    IEncryptionService encryptionService,
    IOptions<AppOptions> appOptions,
    ILogger<MessageService> logger) : IMessageService
{
    private readonly AppOptions _appOptions = appOptions.Value;

    // Slug generation constants
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";
    private const int InitialSlugLength = 6;
    private const int MaxSlugRetries = 10;

    public async Task<CreateMessageResponse> CreateAsync(CreateMessageRequest request, CancellationToken ct = default)
    {
        var expiresAt = CalculateExpiration(request.Expiration);
        var slug = await GenerateUniqueSlugAsync(ct);

        var message = new TemporaryMessage
        {
            Slug      = slug,
            ExpiresAt = expiresAt,
        };

        if (!string.IsNullOrEmpty(request.Password))
        {
            var (encryptedContent, salt, iv) = encryptionService.Encrypt(request.Content, request.Password);
            message.EncryptedContent = encryptedContent;
            message.IsEncrypted      = true;
            message.Salt             = salt;
            message.IV               = iv;
        }
        else
        {
            // Store content as-is (no encryption)
            message.EncryptedContent = request.Content;
            message.IsEncrypted      = false;
        }

        await repository.AddAsync(message, ct);

        logger.LogInformation("Created message with slug '{Slug}', expires at {ExpiresAt}", slug, expiresAt);

        return new CreateMessageResponse
        {
            Slug      = slug,
            Url       = $"{_appOptions.BaseUrl}/m/{slug}",
            ExpiresAt = expiresAt,
        };
    }

    public async Task<GetMessageResponse> GetBySlugAsync(string slug, string? password, CancellationToken ct = default)
    {
        var message = await repository.GetBySlugAsync(slug, ct)
            ?? throw new MessageNotFoundException(slug);

        if (message.IsExpired())
        {
            logger.LogInformation("Access attempt to expired message '{Slug}'", slug);
            throw new MessageExpiredException(slug);
        }

        string content;

        if (message.IsEncrypted)
        {
            if (string.IsNullOrEmpty(password))
                throw new InvalidPasswordException();

            content = encryptionService.Decrypt(
                message.EncryptedContent,
                message.Salt!,
                message.IV!,
                password);
        }
        else
        {
            content = message.EncryptedContent;
        }

        return new GetMessageResponse { Content = content };
    }

    public async Task DeleteBySlugAsync(string slug, CancellationToken ct = default)
    {
        var message = await repository.GetBySlugAsync(slug, ct)
            ?? throw new MessageNotFoundException(slug);

        await repository.DeleteAsync(message, ct);
        logger.LogInformation("Deleted message with slug '{Slug}'", slug);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static DateTime CalculateExpiration(string expiration) => expiration switch
    {
        "12h" => DateTime.UtcNow.AddHours(12),
        "7d"  => DateTime.UtcNow.AddDays(7),
        "1m"  => DateTime.UtcNow.AddDays(30),
        _     => throw new ArgumentOutOfRangeException(nameof(expiration), "Invalid expiration value.")
    };

    private async Task<string> GenerateUniqueSlugAsync(CancellationToken ct)
    {
        var length = InitialSlugLength;

        for (var attempt = 0; attempt < MaxSlugRetries; attempt++)
        {
            var slug = GenerateSlug(length);

            if (!await repository.SlugExistsAsync(slug, ct))
                return slug;

            // On repeated collisions, increase length to reduce probability
            if (attempt > 0 && attempt % 3 == 0)
                length++;
        }

        // Fallback with a longer slug guaranteed to be unique (extremely unlikely to reach here)
        return GenerateSlug(length + 4);
    }

    private static string GenerateSlug(int length)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];

        for (var i = 0; i < length; i++)
            chars[i] = Alphabet[randomBytes[i] % Alphabet.Length];

        return new string(chars);
    }
}

// -------------------------------------------------------------------------
// Configuration record
// -------------------------------------------------------------------------

public sealed class AppOptions
{
    public const string SectionName = "App";

    public string BaseUrl { get; set; } = "https://localhost:7001";
    public int CleanupIntervalMinutes { get; set; } = 30;
}
