namespace Http3Streams.Protocol;

// RFC 9000 §16. The first two bits select the integer's width in bytes.
internal static class QuicInteger
{
    public static async ValueTask<long?> TryReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] bytes = new byte[8];
        if (await stream.ReadAsync(bytes.AsMemory(0, 1), cancellationToken) == 0)
        {
            return null;
        }

        int width = 1 << (bytes[0] >> 6);
        await stream.ReadExactlyAsync(bytes.AsMemory(1, width - 1), cancellationToken);
        long value = bytes[0] & 0x3f;
        for (int i = 1; i < width; i++)
        {
            value = (value << 8) | bytes[i];
        }

        return value;
    }

    public static async ValueTask<long> ReadAsync(Stream stream, CancellationToken cancellationToken) =>
        await TryReadAsync(stream, cancellationToken) ?? throw new EndOfStreamException();

    public static void Write(Stream destination, int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        int width = value switch
        {
            < 64 => 1,
            < 16384 => 2,
            < (1 << 30) => 4,
            _ => 8
        };
        Span<byte> bytes = stackalloc byte[8];
        for (int i = width - 1; i >= 0; i--)
        {
            bytes[i] = (byte)value;
            value >>= 8;
        }

        bytes[0] |= width switch
        {
            2 => (byte)0x40,
            4 => (byte)0x80,
            8 => (byte)0xc0,
            _ => (byte)0
        };
        destination.Write(bytes[..width]);
    }
}
