namespace Api.Services.Interfaces;

public interface IEncryptionService
{
    /// <summary>
    /// Encrypts <paramref name="content"/> using AES-256-CBC with a key derived from
    /// <paramref name="password"/> via PBKDF2.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    ///   <item><c>encryptedContent</c> — Base64-encoded ciphertext</item>
    ///   <item><c>salt</c> — Base64-encoded random salt (32 bytes)</item>
    ///   <item><c>iv</c> — Base64-encoded AES IV (16 bytes)</item>
    /// </list>
    /// </returns>
    (string encryptedContent, string salt, string iv) Encrypt(string content, string password);

    /// <summary>
    /// Decrypts a previously encrypted message.
    /// </summary>
    /// <exception cref="Api.Exceptions.InvalidPasswordException">
    /// Thrown when the password is incorrect or the ciphertext is corrupted.
    /// </exception>
    string Decrypt(string encryptedContent, string salt, string iv, string password);
}
