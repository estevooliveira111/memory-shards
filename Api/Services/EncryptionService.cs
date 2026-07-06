using System.Security.Cryptography;
using System.Text;
using Api.Exceptions;
using Api.Services.Interfaces;

namespace Api.Services;

/// <summary>
/// AES-256-CBC encryption with PBKDF2-SHA256 key derivation.
/// Each message uses a unique 32-byte salt and 16-byte IV.
/// </summary>
public sealed class EncryptionService : IEncryptionService
{
    // PBKDF2 parameters — tuned for ~100ms on modern hardware while resisting brute-force
    private const int Pbkdf2Iterations = 200_000;
    private const int KeySizeBytes = 32; // 256-bit AES key
    private const int SaltSizeBytes = 32;
    private const int IvSizeBytes = 16;  // AES block size

    public (string encryptedContent, string salt, string iv) Encrypt(string content, string password)
    {
        // Generate cryptographically secure random salt and IV
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var ivBytes   = RandomNumberGenerator.GetBytes(IvSizeBytes);

        var key = DeriveKey(password, saltBytes);

        using var aes = CreateAes(key, ivBytes);
        using var encryptor = aes.CreateEncryptor();

        var contentBytes = Encoding.UTF8.GetBytes(content);
        var cipherBytes  = encryptor.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

        return (
            Convert.ToBase64String(cipherBytes),
            Convert.ToBase64String(saltBytes),
            Convert.ToBase64String(ivBytes)
        );
    }

    public string Decrypt(string encryptedContent, string salt, string iv, string password)
    {
        try
        {
            var saltBytes   = Convert.FromBase64String(salt);
            var ivBytes     = Convert.FromBase64String(iv);
            var cipherBytes = Convert.FromBase64String(encryptedContent);

            var key = DeriveKey(password, saltBytes);

            using var aes = CreateAes(key, ivBytes);
            using var decryptor = aes.CreateDecryptor();

            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException)
        {
            // Wrong key → wrong password
            throw new InvalidPasswordException();
        }
        catch (FormatException)
        {
            // Corrupted Base64 data
            throw new InvalidPasswordException();
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        return Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            KeySizeBytes);
    }

    private static Aes CreateAes(byte[] key, byte[] iv)
    {
        var aes = Aes.Create();
        aes.KeySize  = 256;
        aes.Mode     = CipherMode.CBC;
        aes.Padding  = PaddingMode.PKCS7;
        aes.Key      = key;
        aes.IV       = iv;
        return aes;
    }
}
