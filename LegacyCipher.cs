using System.Security.Cryptography;

sealed class LegacyCipher(byte[] mac, byte[] key, byte[] initialIv, byte minor = 0) : IDisposable
{
    readonly SymmetricAlgorithm cipher = Create(key);
    readonly byte[] macSecret = mac.ToArray();
    byte[] iv = initialIv.ToArray();
    ulong sequence;
    static SymmetricAlgorithm Create(byte[] key) { SymmetricAlgorithm cipher = key.Length == 16 ? Aes.Create() : TripleDES.Create(); cipher.Key = key; return cipher; }
    byte[] Mac(byte type, byte[] data)
    {
        if (sequence == ulong.MaxValue) throw new IOException("SSL sequence limit.");
        return minor == 0 ? Ssl3Crypto.Mac(macSecret, sequence++, type, data) : Tls10Crypto.Mac(macSecret, sequence++, type, data);
    }
    public byte[] Seal(byte type, byte[] data)
    {
        byte[] plain = [.. data, .. Mac(type, data)];
        var size = cipher.BlockSize / 8;
        var padding = size - 1 - plain.Length % size;
        byte[] padded = [.. plain, .. Enumerable.Repeat((byte)padding, padding + 1)];
        var encrypted = cipher.EncryptCbc(padded, iv, PaddingMode.None);
        iv = encrypted[^size..]; return encrypted;
    }
    public byte[] Open(byte type, byte[] encrypted)
    {
        var size = cipher.BlockSize / 8;
        if (encrypted.Length == 0 || encrypted.Length % size != 0) throw new IOException("Invalid SSL CBC record.");
        var plain = cipher.DecryptCbc(encrypted, iv, PaddingMode.None); iv = encrypted[^size..];
        var length = plain.Length - plain[^1] - 21;
        if ((minor == 0 && plain[^1] >= size) || length < 0) throw new IOException("Invalid SSL CBC record.");
        if (minor != 0)
            foreach (var value in plain.AsSpan(length + 20)) if (value != plain[^1]) throw new IOException("Invalid TLS CBC padding.");
        var data = plain[..length];
        if (!CryptographicOperations.FixedTimeEquals(Mac(type, data), plain.AsSpan(length, 20))) throw new IOException("Invalid SSL CBC record.");
        return data;
    }
    public void Dispose() { cipher.Dispose(); CryptographicOperations.ZeroMemory(macSecret); CryptographicOperations.ZeroMemory(iv); }
}
