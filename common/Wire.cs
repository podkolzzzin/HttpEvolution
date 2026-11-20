// The TLS length-prefixed byte vectors, without a protocol library.
sealed class Wire(byte[] data)
{
    int offset;
    public int Left => data.Length - offset;

    public byte[] Take(int length)
    {
        if (length < 0 || length > Left)
        {
            throw new InvalidDataException("Truncated vector.");
        }

        var bytes = data.AsSpan(offset, length).ToArray();
        offset += length;
        return bytes;
    }

    public int Int(int width = 2) => Take(width).Aggregate(0, (value, octet) => (value << 8) | octet);
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

        return Enumerable.Range(0, width).Select(i => (byte)(value >> (8 * (width - i - 1)))).ToArray();
    }

    public static byte[] Vector(byte[] bytes, int width = 2) => [.. Size(bytes.Length, width), .. bytes];
    public static byte[] Message(byte type, byte[] bytes) => [type, .. Vector(bytes, 3)];
    public static byte[] Extension(int type, byte[] bytes) => [.. Size(type), .. Vector(bytes)];
}
