using System.Security.Cryptography;
using System.Text;

namespace CourseManagement.Utils;

public static class EmailSecurity
{
    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public static string HashEmail(string email)
    {
        string? secretKey = Environment.GetEnvironmentVariable("HMAC_EMAIL_SECRET");

        Console.WriteLine($"Is secret key null? {secretKey == null}");
        Console.WriteLine($"Is secret key empty? {string.IsNullOrWhiteSpace(secretKey)}");

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("HMAC_EMAIL_SECRET environment variable needs to be string.");
        }

        byte[] keyBytes = Convert.FromBase64String(secretKey);
        if (keyBytes.Length != 32)
        {
            throw new InvalidOperationException("HMAC_EMAIL_SECRET must be exactly 32 bytes.");
        }
        var NormalizedEmail = NormalizeEmail(email);
        byte[] emailBytes = Encoding.UTF8.GetBytes(NormalizedEmail);
        byte[] hashBytes = HMACSHA256.HashData(keyBytes, emailBytes);
        return Convert.ToHexString(hashBytes);
    }

    public static (string EncryptedEmail, string IV, string AuthTag) EncryptEmail(string email)
    {
        string? secretKey = Environment.GetEnvironmentVariable("EMAIL_ENCRYPTION_KEY");

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("EMAIL_ENCRYPTION_KEY environment variable is missing.");
        }

        byte[] keyBytes = Convert.FromBase64String(secretKey);

        if (keyBytes.Length != 32)
        {
            throw new InvalidOperationException("EMAIL_ENCRYPTION_KEY must be exactly 32 bytes.");
        }

        byte[] iv = RandomNumberGenerator.GetBytes(12);
        byte[] plaintext = Encoding.UTF8.GetBytes(NormalizeEmail(email));

        byte[] ciphertext = new byte[plaintext.Length];
        byte[] authTag = new byte[16];

        using var aes = new AesGcm(keyBytes, 16);

        aes.Encrypt(iv, plaintext, ciphertext, authTag);

        return (
            Convert.ToBase64String(ciphertext),
            Convert.ToBase64String(iv),
            Convert.ToBase64String(authTag)
        );
    }

    public static string DecryptEmail(string encryptedEmail, string iv, string authTag)
    {
        string? secretKey = Environment.GetEnvironmentVariable("EMAIL_ENCRYPTION_KEY");

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("EMAIL_ENCRYPTION_KEY environment variable is missing.");
        }

        byte[] keyBytes = Convert.FromBase64String(secretKey);

        if (keyBytes.Length != 32)
        {
            throw new InvalidOperationException("EMAIL_ENCRYPTION_KEY must be exactly 32 bytes.");
        }

        byte[] ciphertext = Convert.FromBase64String(encryptedEmail);
        byte[] ivBytes = Convert.FromBase64String(iv);
        byte[] authTagBytes = Convert.FromBase64String(authTag);

        if (ivBytes.Length != 12 || authTagBytes.Length != 16)
        {
            throw new InvalidOperationException("Invalid IV or authentication tag length.");
        }

        byte[] plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(keyBytes, 16);

        aes.Decrypt(ivBytes, ciphertext, authTagBytes, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }


}




