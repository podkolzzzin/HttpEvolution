namespace Http3Streams.Quic;

// QUIC/HTTP/3 integers use 1, 2, 4 or 8 bytes; the top two bits select the width.
internal sealed class QuicData(byte[] data)
{
    public int Offset
    {
        get; private set;
    }
    public int Left => data.Length - Offset;

    public byte[] Take(int length)
    {
        if (length < 0 || length > Left)
        {
            throw new InvalidDataException("Truncated QUIC field.");
        }

        var bytes = data.AsSpan(Offset, length).ToArray();
        Offset += length;
        return bytes;
    }

    public byte Byte() => Left > 0 ? data[Offset++] : throw new InvalidDataException("Truncated QUIC field.");
    public ulong Int()
    {
        var first = Byte();
        ulong value = (ulong)(first & 63);
        for (int index = 1; index < 1 << (first >> 6); index++)
        {
            value = (value << 8) | Byte();
        }

        return value;
    }

    public byte[] Vector()
    {
        var length = Int();
        if (length > (ulong)Left)
        {
            throw new InvalidDataException("Truncated QUIC vector.");
        }

        return Take((int)length);
    }

    public static byte[] Int(ulong value)
    {
        var width = value < 64
            ? 1
            : value < 16384
                ? 2
                : value < (1UL << 30)
                    ? 4
                    : value < (1UL << 62)
                        ? 8
                        : throw new InvalidDataException("QUIC integer overflow.");
        var bytes = new byte[width];
        for (int i = width - 1; i >= 0; i--)
        {
            bytes[i] = (byte)value;
            value >>= 8;
        }

        bytes[0] |= (byte)(width == 1 ? 0 : width == 2 ? 64 : width == 4 ? 128 : 192);
        return bytes;
    }

    public static byte[] Vector(byte[] bytes) => [.. Int((ulong)bytes.Length), .. bytes];
    public static byte[] Pair(ulong id, byte[] bytes) => [.. Int(id), .. Vector(bytes)];
}
