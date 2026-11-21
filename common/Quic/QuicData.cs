// QUIC/HTTP/3 integers use 1, 2, 4 or 8 bytes; the top two bits select the width.
internal sealed class QuicData(byte[] data)
{
    private readonly Wire input = new(data);
    public int Left => input.Left;
    public int Offset => data.Length - Left;

    public byte[] Take(int length) => input.Take(length);
    public byte Byte() => (byte)input.Int(1);
    public ulong Int()
    {
        var first = Byte();
        ulong value = (ulong)(first & 63);
        foreach (var octet in input.TakeSpan((1 << (first >> 6)) - 1))
        {
            value = (value << 8) | octet;
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
        var bytes = Wire.BigEndian(value, width);
        bytes[0] |= (byte)(width == 1 ? 0 : width == 2 ? 64 : width == 4 ? 128 : 192);
        return bytes;
    }

    public static byte[] Vector(byte[] bytes) => [.. Int((ulong)bytes.Length), .. bytes];
    public static byte[] Pair(ulong id, byte[] bytes) => [.. Int(id), .. Vector(bytes)];
}
