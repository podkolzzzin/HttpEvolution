using System.Buffers.Binary;

internal enum Http2FrameType : byte
{
    Data = 0,
    Headers = 1,
    ResetStream = 3,
    Settings = 4,
    Ping = 6,
    GoAway = 7,
    WindowUpdate = 8,
    Continuation = 9
}

[Flags]
internal enum Http2Flags : byte
{
    None = 0,
    Ack = 0x01,
    EndStream = 0x01,
    EndHeaders = 0x04,
    Padded = 0x08,
    Priority = 0x20
}

internal readonly record struct Http2Frame(Http2FrameType Type, Http2Flags Flags, int StreamId, byte[] Payload)
{
    public bool HasFlags(Http2Flags flags) => (Flags & flags) == flags;
}

internal static class Http2Frames
{
    public const int ConnectionStreamId = 0;
    public const int MaxFramePayload = 16_384;
    public const int MaxStreamId = 0x7fff_ffff;

    private const int HeaderLength = 9;
    private const int TypeOffset = 3;
    private const int FlagsOffset = 4;
    private const int StreamIdOffset = 5;

    public static Http2Frame Read(Stream stream)
    {
        Span<byte> header = stackalloc byte[HeaderLength];
        stream.ReadExactly(header);
        var length = (header[0] << 16) | (header[1] << 8) | header[2];
        if (length > MaxFramePayload)
        {
            throw new InvalidDataException("Frame exceeds the advertised maximum.");
        }

        var payload = new byte[length];
        stream.ReadExactly(payload);
        return new(
            (Http2FrameType)header[TypeOffset],
            (Http2Flags)header[FlagsOffset],
            BinaryPrimitives.ReadInt32BigEndian(header[StreamIdOffset..]) & MaxStreamId,
            payload
        );
    }

    public static void Write(Stream stream, Http2FrameType type, Http2Flags flags, int streamId, ReadOnlySpan<byte> payload)
    {
        Span<byte> header = stackalloc byte[HeaderLength];
        header[0] = (byte)(payload.Length >> 16);
        header[1] = (byte)(payload.Length >> 8);
        header[2] = (byte)payload.Length;
        header[TypeOffset] = (byte)type;
        header[FlagsOffset] = (byte)flags;
        BinaryPrimitives.WriteInt32BigEndian(header[StreamIdOffset..], streamId);
        stream.Write(header);
        stream.Write(payload);
    }
}
