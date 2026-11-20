using System.Security.Cryptography;
using System.Text;

// RFC 8446 section 7: HKDF-SHA256 with TLS 1.3 labels.
static class TlsCrypto
{
    public static byte[] Extract(byte[] salt, byte[] input) => HMACSHA256.HashData(salt, input);
    public static byte[] Label(byte[] secret, string label, byte[] context, int length = 32)
    {
        var name = Encoding.ASCII.GetBytes("tls13 " + label);
        byte[] info = [(byte)(length >> 8), (byte)length, (byte)name.Length, .. name, (byte)context.Length, .. context];
        return HKDF.Expand(HashAlgorithmName.SHA256, secret, length, info);
    }
    public static byte[] Derive(byte[] secret, string label, byte[] transcript) => Label(secret, label, SHA256.HashData(transcript));
    public static byte[] Finished(byte[] secret, byte[] transcript) => HMACSHA256.HashData(Label(secret, "finished", []), SHA256.HashData(transcript));
}

sealed class TlsCipher : IDisposable
{
    readonly AesGcm aes;
    readonly byte[] iv;
    public TlsCipher(byte[] secret, string prefix = "")
    {
        aes = new AesGcm(TlsCrypto.Label(secret, prefix + "key", [], 16), 16);
        iv = TlsCrypto.Label(secret, prefix + "iv", [], 12);
    }
    byte[] Nonce(ulong sequence)
    {
        var nonce = iv.ToArray();
        for (var i = 0; i < 8; i++) nonce[^(i + 1)] ^= (byte)(sequence >> (8 * i));
        return nonce;
    }
    public byte[] Seal(ulong sequence, byte[] plain, byte[] additional)
    {
        var result = new byte[plain.Length + 16];
        aes.Encrypt(Nonce(sequence), plain, result.AsSpan(0, plain.Length), result.AsSpan(plain.Length), additional);
        return result;
    }
    public byte[] Open(ulong sequence, byte[] encrypted, byte[] additional)
    {
        if (encrypted.Length < 16) throw new InvalidDataException("Truncated AES-GCM tag.");
        var result = new byte[encrypted.Length - 16];
        aes.Decrypt(Nonce(sequence), encrypted.AsSpan(0, result.Length), encrypted.AsSpan(result.Length), result, additional);
        return result;
    }
    public void Dispose() => aes.Dispose();
}
