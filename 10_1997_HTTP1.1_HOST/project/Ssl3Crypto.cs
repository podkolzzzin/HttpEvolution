using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

// The historical SSL 3.0 constructions from RFC 6101, before TLS PRF/HMAC.
static class Ssl3Crypto
{
    static byte[] Pad(int length, byte value) => Enumerable.Repeat(value, length).ToArray();
    public static byte[] Expand(byte[] secret, byte[] seed, int length)
    {
        using var output = new MemoryStream();
        for (var i = 0; output.Length < length; i++)
            output.Write(MD5.HashData([.. secret, .. SHA1.HashData([.. Pad(i + 1, (byte)('A' + i)), .. secret, .. seed])]));
        return output.ToArray()[..length];
    }
    public static byte[] Finished(byte[] master, byte[] transcript, bool client)
    {
        var sender = Encoding.ASCII.GetBytes(client ? "CLNT" : "SRVR");
        byte[] Part(Func<byte[], byte[]> hash, int width) => hash([.. master, .. Pad(width, 0x5c), .. hash([.. transcript, .. sender, .. master, .. Pad(width, 0x36)])]);
        return [.. Part(MD5.HashData, 48), .. Part(SHA1.HashData, 40)];
    }
    public static byte[] Mac(byte[] secret, ulong sequence, byte type, byte[] data)
    {
        var number = new byte[8]; BinaryPrimitives.WriteUInt64BigEndian(number, sequence);
        var inner = SHA1.HashData([.. secret, .. Pad(40, 0x36), .. number, type, .. Wire.Size(data.Length), .. data]);
        return SHA1.HashData([.. secret, .. Pad(40, 0x5c), .. inner]);
    }
}
