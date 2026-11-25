namespace Http3Streams.Protocol;

internal sealed record Http3Frame(long Type, byte[] Payload)
{
    public const int Data = 0;
    public const int Headers = 1;
    public const int MaxPayload = 64 * 1024;

    public static async Task<Http3Frame?> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        long? type = await QuicInteger.TryReadAsync(stream, cancellationToken);
        if (type is null)
        {
            return null;
        }

        long length = await QuicInteger.ReadAsync(stream, cancellationToken);
        if (length > MaxPayload)
        {
            throw new InvalidDataException("The demo accepts frames up to 64 KiB.");
        }

        byte[] payload = new byte[(int)length];
        await stream.ReadExactlyAsync(payload, cancellationToken);
        return new Http3Frame(type.Value, payload);
    }

    public static async Task WriteAsync(Stream stream, int type, byte[] payload, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(payload.Length, MaxPayload);
        using var prefix = new MemoryStream(9);
        QuicInteger.Write(prefix, type);
        QuicInteger.Write(prefix, payload.Length);
        await stream.WriteAsync(prefix.GetBuffer().AsMemory(0, (int)prefix.Length), cancellationToken);
        await stream.WriteAsync(payload, cancellationToken);
    }
}
