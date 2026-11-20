using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

static class Tls10Crypto
{
    static byte[] Part(byte[] secret, byte[] seed, int length, bool md5)
    {
        Func<byte[], byte[], byte[]> hmac = md5 ? HMACMD5.HashData : HMACSHA1.HashData;
        using var output = new MemoryStream(); var a = seed;
        while (output.Length < length) { a = hmac(secret, a); output.Write(hmac(secret, [.. a, .. seed])); }
        return output.ToArray()[..length];
    }
    public static byte[] Prf(byte[] secret, string label, byte[] seed, int length)
    {
        seed = [.. Encoding.ASCII.GetBytes(label), .. seed]; var half = (secret.Length + 1) / 2;
        var left = Part(secret[..half], seed, length, true); var right = Part(secret[(secret.Length - half)..], seed, length, false);
        for (var i = 0; i < length; i++) left[i] ^= right[i]; return left;
    }
    public static byte[] Finished(byte[] master, byte[] transcript, bool client) => Prf(master, client ? "client finished" : "server finished", [.. MD5.HashData(transcript), .. SHA1.HashData(transcript)], 12);
    public static byte[] Mac(byte[] key, ulong sequence, byte type, byte[] data)
    {
        var number = new byte[8]; BinaryPrimitives.WriteUInt64BigEndian(number, sequence);
        byte[] message = [.. number, type, 3, 1, .. Wire.Size(data.Length), .. data];
        return HMACSHA1.HashData(key, message);
    }
}
