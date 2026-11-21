using System.Buffers.Binary;

// The TLS length-prefixed byte vectors, without a protocol library.
internal sealed class Wire(byte[] data)
{
    private int offset;
    public int Left => data.Length - offset;

    public byte[] Take(int length) => TakeSpan(length).ToArray();

    public ReadOnlySpan<byte> TakeSpan(int length)
    {
        if (length < 0 || length > Left)
        {
            throw new InvalidDataException("Truncated vector.");
        }

        var bytes = data.AsSpan(offset, length);
        offset += length;
        return bytes;
    }

    public int Int(int width = 2)
    {
        var value = 0;
        foreach (var octet in TakeSpan(width))
        {
            value = (value << 8) | octet;
        }

        return value;
    }

    public ushort[] UInt16Vector()
    {
        var bytes = TakeSpan(Int());
        if (bytes.Length % sizeof(ushort) != 0)
        {
            throw new InvalidDataException("Truncated 16-bit vector entry.");
        }

        var values = new ushort[bytes.Length / sizeof(ushort)];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = BinaryPrimitives.ReadUInt16BigEndian(bytes[(i * sizeof(ushort))..]);
        }

        return values;
    }

    public byte[] Vector(int width = 2) => Take(Int(width));
    public void End()
    {
        if (Left != 0)
        {
            throw new InvalidDataException("Trailing vector bytes.");
        }
    }

    public static byte[] Size(int value, int width = 2)
    {
        if (width is < 1 or > 3 || value < 0 || value >= (1 << (8 * width)))
        {
            throw new InvalidDataException("Vector too large.");
        }

        return BigEndian((ulong)value, width);
    }

    public static byte[] BigEndian(ulong value, int width)
    {
        var bytes = new byte[width];
        for (var i = width - 1; i >= 0; i--)
        {
            bytes[i] = (byte)value;
            value >>= 8;
        }

        return bytes;
    }

    public static byte[] Join(byte[][] parts)
    {
        var length = 0;
        foreach (var part in parts)
        {
            length = checked(length + part.Length);
        }

        var bytes = new byte[length];
        var start = 0;
        foreach (var part in parts)
        {
            part.AsSpan().CopyTo(bytes.AsSpan(start));
            start += part.Length;
        }

        return bytes;
    }

    public static byte[] Vector(byte[] bytes, int width = 2) => [.. Size(bytes.Length, width), .. bytes];
    public static byte[] Message(byte type, byte[] bytes) => [type, .. Vector(bytes, 3)];
    public static byte[] Extension(int type, byte[] bytes) => [.. Size(type), .. Vector(bytes)];
}
