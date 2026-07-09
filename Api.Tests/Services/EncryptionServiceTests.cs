using Api.Exceptions;
using Api.Services;

namespace Api.Tests.Services;

public class EncryptionServiceTests
{
    private readonly EncryptionService _sut = new();

    [Fact]
    public void Encrypt_ThenDecrypt_WithCorrectPassword_ReturnsOriginalContent()
    {
        const string content = "segredo super importante";
        const string password = "1234";

        var (encrypted, salt, iv) = _sut.Encrypt(content, password);
        var decrypted = _sut.Decrypt(encrypted, salt, iv, password);

        Assert.Equal(content, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentSaltAndIvOnEachCall()
    {
        var (_, salt1, iv1) = _sut.Encrypt("mesmo conteúdo", "1234");
        var (_, salt2, iv2) = _sut.Encrypt("mesmo conteúdo", "1234");

        Assert.NotEqual(salt1, salt2);
        Assert.NotEqual(iv1, iv2);
    }

    [Fact]
    public void Decrypt_WithWrongPassword_ThrowsInvalidPasswordException()
    {
        var (encrypted, salt, iv) = _sut.Encrypt("segredo", "1234");

        Assert.Throws<InvalidPasswordException>(() => _sut.Decrypt(encrypted, salt, iv, "9999"));
    }

    [Fact]
    public void Decrypt_WithCorruptedCiphertext_ThrowsInvalidPasswordException()
    {
        var (_, salt, iv) = _sut.Encrypt("segredo", "1234");

        Assert.Throws<InvalidPasswordException>(() => _sut.Decrypt("not-valid-base64!!", salt, iv, "1234"));
    }

    [Fact]
    public void Decrypt_WithCorruptedSalt_ThrowsInvalidPasswordException()
    {
        var (encrypted, _, iv) = _sut.Encrypt("segredo", "1234");

        Assert.Throws<InvalidPasswordException>(() => _sut.Decrypt(encrypted, "not-valid-base64!!", iv, "1234"));
    }

    [Fact]
    public void Decrypt_WithTamperedCiphertext_ThrowsInvalidPasswordException()
    {
        var (encrypted, salt, iv) = _sut.Encrypt("segredo", "1234");

        // Flip the ciphertext bytes so decryption fails padding/authentication
        var bytes = Convert.FromBase64String(encrypted);
        bytes[0] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);

        Assert.Throws<InvalidPasswordException>(() => _sut.Decrypt(tampered, salt, iv, "1234"));
    }

    [Fact]
    public void Encrypt_EmptyContent_RoundTripsSuccessfully()
    {
        var (encrypted, salt, iv) = _sut.Encrypt(string.Empty, "1234");
        var decrypted = _sut.Decrypt(encrypted, salt, iv, "1234");

        Assert.Equal(string.Empty, decrypted);
    }
}
