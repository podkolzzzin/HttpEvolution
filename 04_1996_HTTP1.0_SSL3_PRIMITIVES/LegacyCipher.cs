using System.Security.Cryptography;

internal sealed class LegacyCipher(byte[] mac, byte[] key, byte[] initialIv) : IDisposable
{
    private readonly TripleDES cipher = Create(key);
    private readonly byte[] macSecret = mac.AsSpan().ToArray();
    private byte[] iv = initialIv.AsSpan().ToArray();
    private ulong sequence;
    private static TripleDES Create(byte[] key)
    {
        var cipher = TripleDES.Create();
        cipher.Key = key;
        return cipher;
    }

    private byte[] Mac(byte type, byte[] data)
    {
        if (sequence == ulong.MaxValue)
        {
            throw new IOException("SSL sequence limit.");
        }

        return Ssl3Crypto.Mac(macSecret, sequence++, type, data);
    }

    public byte[] Seal(byte type, byte[] data)
    {
        byte[] plain = [.. data, .. Mac(type, data)];
        var padding = 7 - plain.Length % 8;
        var padded = new byte[plain.Length + padding + 1];
        plain.AsSpan().CopyTo(padded);
        padded.AsSpan(plain.Length).Fill((byte)padding);
        var encrypted = cipher.EncryptCbc(padded, iv, PaddingMode.None);
        iv = encrypted[^8..];
        return encrypted;
    }

    public byte[] Open(byte type, byte[] encrypted)
    {
        if (encrypted.Length == 0 || encrypted.Length % 8 != 0)
        {
            throw new IOException("Invalid SSL CBC record.");
        }

        var plain = cipher.DecryptCbc(encrypted, iv, PaddingMode.None);
        iv = encrypted[^8..];
        var length = plain.Length - plain[^1] - 21;
        if (plain[^1] >= 8 || length < 0)
        {
            throw new IOException("Invalid SSL CBC record.");
        }

        var data = plain[..length];
        if (!CryptographicOperations.FixedTimeEquals(Mac(type, data), plain.AsSpan(length, 20)))
        {
            throw new IOException("Invalid SSL CBC record.");
        }

        return data;
    }

    public void Dispose()
    {
        cipher.Dispose();
        CryptographicOperations.ZeroMemory(macSecret);
        CryptographicOperations.ZeroMemory(iv);
    }
}
