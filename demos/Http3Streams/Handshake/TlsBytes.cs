namespace Http3Streams.Handshake;

// TLS integers and length-prefixed vectors are big-endian (RFC 8446 §3).
internal sealed class TlsBytes(byte[] data)
{
    private int offset;
    public int Remaining => data.Length - offset;

    public byte[] Read(int length)
    {
        if (length > Remaining)
        {
            throw new InvalidDataException("Truncated TLS message.");
        }

        var bytes = data[offset..(offset + length)];
        offset += length;
        return bytes;
    }

    public int ReadInt(int width = 2)
    {
        var value = 0;
        foreach (byte octet in Read(width))
        {
            value = (value << 8) | octet;
        }
        return value;
    }

    public byte[] ReadVector(int width = 2) => Read(ReadInt(width));

    public void End()
    {
        if (Remaining != 0)
        {
            throw new InvalidDataException("Unexpected bytes at the end of a TLS field.");
        }
    }

    public Dictionary<int, byte[]> ReadExtensions()
    {
        var fields = new TlsBytes(ReadVector());
        var result = new Dictionary<int, byte[]>();
        while (fields.Remaining > 0)
        {
            if (!result.TryAdd(fields.ReadInt(), fields.ReadVector()))
            {
                throw new InvalidDataException("Duplicate TLS extension.");
            }
        }
        return result;
    }

    public static byte[] Required(Dictionary<int, byte[]> fields, int type) =>
        fields.GetValueOrDefault(type) ?? throw new InvalidDataException($"Missing TLS extension {type}.");

    public static byte[] Unwrap(byte[] bytes, int width = 2)
    {
        var field = new TlsBytes(bytes);
        var result = field.ReadVector(width);
        field.End();
        return result;
    }

    public static byte[] Vector(byte[] bytes, int width = 2)
    {
        if (bytes.Length >= (1 << (8 * width)))
        {
            throw new InvalidDataException("TLS field exceeds its length prefix.");
        }

        var vector = new byte[width + bytes.Length];
        for (int i = 0; i < width; i++)
        {
            vector[width - 1 - i] = (byte)(bytes.Length >> (8 * i));
        }
        bytes.CopyTo(vector, width);
        return vector;
    }

    public static byte[] Message(byte type, byte[] body) => [type, .. Vector(body, 3)];

    public static byte[] Extension(int type, byte[] data) => [(byte)(type >> 8), (byte)type, .. Vector(data)];
}
