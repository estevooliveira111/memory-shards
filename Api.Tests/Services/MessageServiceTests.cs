using Api.DTOs;
using Api.Entities;
using Api.Exceptions;
using Api.Repositories.Interfaces;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Api.Tests.Services;

public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _repository = new();
    private readonly Mock<IEncryptionService> _encryptionService = new();
    private readonly MessageService _sut;

    public MessageServiceTests()
    {
        var options = Options.Create(new AppOptions { BaseUrl = "https://memory-shards.test" });
        _sut = new MessageService(
            _repository.Object,
            _encryptionService.Object,
            options,
            NullLogger<MessageService>.Instance);
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_WithoutPassword_StoresPlainContentAndDoesNotEncrypt()
    {
        _repository.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        TemporaryMessage? saved = null;
        _repository.Setup(r => r.AddAsync(It.IsAny<TemporaryMessage>(), It.IsAny<CancellationToken>()))
            .Callback<TemporaryMessage, CancellationToken>((m, _) => saved = m)
            .ReturnsAsync((TemporaryMessage m, CancellationToken _) => m.Slug);

        var request = new CreateMessageRequest { Content = "olá mundo", Expiration = "12h" };

        var response = await _sut.CreateAsync(request);

        Assert.NotNull(saved);
        Assert.False(saved!.IsEncrypted);
        Assert.Equal("olá mundo", saved.EncryptedContent);
        Assert.Null(saved.Salt);
        Assert.Null(saved.IV);
        Assert.Equal($"https://memory-shards.test/m/{response.Slug}", response.Url);
        _encryptionService.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithPassword_EncryptsContentUsingEncryptionService()
    {
        _repository.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _encryptionService.Setup(e => e.Encrypt("conteúdo secreto", "1234"))
            .Returns(("cipher", "salt", "iv"));

        TemporaryMessage? saved = null;
        _repository.Setup(r => r.AddAsync(It.IsAny<TemporaryMessage>(), It.IsAny<CancellationToken>()))
            .Callback<TemporaryMessage, CancellationToken>((m, _) => saved = m)
            .ReturnsAsync((TemporaryMessage m, CancellationToken _) => m.Slug);

        var request = new CreateMessageRequest { Content = "conteúdo secreto", Expiration = "7d", Password = "1234" };

        await _sut.CreateAsync(request);

        Assert.NotNull(saved);
        Assert.True(saved!.IsEncrypted);
        Assert.Equal("cipher", saved.EncryptedContent);
        Assert.Equal("salt", saved.Salt);
        Assert.Equal("iv", saved.IV);
    }

    [Theory]
    [InlineData("12h")]
    [InlineData("7d")]
    [InlineData("1m")]
    public async Task CreateAsync_ValidExpirations_AreAcceptedAndInTheFuture(string expiration)
    {
        _repository.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(r => r.AddAsync(It.IsAny<TemporaryMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemporaryMessage m, CancellationToken _) => m.Slug);

        var response = await _sut.CreateAsync(new CreateMessageRequest { Content = "x", Expiration = expiration });

        Assert.True(response.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateAsync_InvalidExpiration_ThrowsArgumentOutOfRangeException()
    {
        var request = new CreateMessageRequest { Content = "x", Expiration = "invalid" };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_RetriesSlugGenerationOnCollision()
    {
        var callCount = 0;
        _repository.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++callCount < 3); // first 2 calls collide, 3rd is free
        _repository.Setup(r => r.AddAsync(It.IsAny<TemporaryMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemporaryMessage m, CancellationToken _) => m.Slug);

        await _sut.CreateAsync(new CreateMessageRequest { Content = "x", Expiration = "12h" });

        _repository.Verify(r => r.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    // -------------------------------------------------------------------------
    // GetBySlugAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetBySlugAsync_MessageDoesNotExist_ThrowsMessageNotFoundException()
    {
        _repository.Setup(r => r.GetBySlugAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemporaryMessage?)null);

        await Assert.ThrowsAsync<MessageNotFoundException>(() => _sut.GetBySlugAsync("missing", null));
    }

    [Fact]
    public async Task GetBySlugAsync_MessageExpired_ThrowsMessageExpiredException()
    {
        var message = new TemporaryMessage
        {
            Slug = "abc123",
            EncryptedContent = "content",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
        };
        _repository.Setup(r => r.GetBySlugAsync("abc123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        await Assert.ThrowsAsync<MessageExpiredException>(() => _sut.GetBySlugAsync("abc123", null));
    }

    [Fact]
    public async Task GetBySlugAsync_EncryptedMessageWithoutPassword_ThrowsInvalidPasswordException()
    {
        var message = new TemporaryMessage
        {
            Slug = "abc123",
            EncryptedContent = "cipher",
            IsEncrypted = true,
            Salt = "salt",
            IV = "iv",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
        _repository.Setup(r => r.GetBySlugAsync("abc123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        await Assert.ThrowsAsync<InvalidPasswordException>(() => _sut.GetBySlugAsync("abc123", null));
    }

    [Fact]
    public async Task GetBySlugAsync_EncryptedMessageWithEmptyPassword_ThrowsInvalidPasswordException()
    {
        var message = new TemporaryMessage
        {
            Slug = "abc123",
            EncryptedContent = "cipher",
            IsEncrypted = true,
            Salt = "salt",
            IV = "iv",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
        _repository.Setup(r => r.GetBySlugAsync("abc123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        await Assert.ThrowsAsync<InvalidPasswordException>(() => _sut.GetBySlugAsync("abc123", ""));
    }

    [Fact]
    public async Task GetBySlugAsync_EncryptedMessageWithCorrectPassword_ReturnsDecryptedContent()
    {
        var message = new TemporaryMessage
        {
            Slug = "abc123",
            EncryptedContent = "cipher",
            IsEncrypted = true,
            Salt = "salt",
            IV = "iv",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
        _repository.Setup(r => r.GetBySlugAsync("abc123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
        _encryptionService.Setup(e => e.Decrypt("cipher", "salt", "iv", "1234"))
            .Returns("plaintext");

        var response = await _sut.GetBySlugAsync("abc123", "1234");

        Assert.Equal("plaintext", response.Content);
    }

    [Fact]
    public async Task GetBySlugAsync_EncryptedMessageWithWrongPassword_PropagatesInvalidPasswordException()
    {
        var message = new TemporaryMessage
        {
            Slug = "abc123",
            EncryptedContent = "cipher",
            IsEncrypted = true,
            Salt = "salt",
            IV = "iv",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
        _repository.Setup(r => r.GetBySlugAsync("abc123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
        _encryptionService.Setup(e => e.Decrypt("cipher", "salt", "iv", "wrong"))
            .Throws<InvalidPasswordException>();

        await Assert.ThrowsAsync<InvalidPasswordException>(() => _sut.GetBySlugAsync("abc123", "wrong"));
    }

    [Fact]
    public async Task GetBySlugAsync_UnencryptedMessage_ReturnsContentAsIsEvenWithoutPassword()
    {
        var message = new TemporaryMessage
        {
            Slug = "abc123",
            EncryptedContent = "plain content",
            IsEncrypted = false,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
        _repository.Setup(r => r.GetBySlugAsync("abc123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var response = await _sut.GetBySlugAsync("abc123", null);

        Assert.Equal("plain content", response.Content);
        _encryptionService.Verify(e => e.Decrypt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // -------------------------------------------------------------------------
    // DeleteBySlugAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteBySlugAsync_MessageDoesNotExist_ThrowsMessageNotFoundException()
    {
        _repository.Setup(r => r.GetBySlugAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemporaryMessage?)null);

        await Assert.ThrowsAsync<MessageNotFoundException>(() => _sut.DeleteBySlugAsync("missing"));
        _repository.Verify(r => r.DeleteAsync(It.IsAny<TemporaryMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBySlugAsync_MessageExists_DeletesItThroughRepository()
    {
        var message = new TemporaryMessage { Slug = "abc123", EncryptedContent = "x" };
        _repository.Setup(r => r.GetBySlugAsync("abc123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        await _sut.DeleteBySlugAsync("abc123");

        _repository.Verify(r => r.DeleteAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }
}
