using static Http2Frames;

internal class Http2Connection(Stream stream)
{
    protected const int SettingsEntryLength = sizeof(ushort) + sizeof(uint);
    private const int PingPayloadLength = 8;
    private static ReadOnlySpan<byte> Preface => "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8;

    public void Run()
    {
        Span<byte> preface = stackalloc byte[Preface.Length];
        stream.ReadExactly(preface);
        if (!preface.SequenceEqual(Preface))
        {
            return;
        }

        Send(Http2FrameType.Settings, Http2Flags.None, ConnectionStreamId, []);
        while (ReadFrame() is { Type: not Http2FrameType.GoAway } frame)
        {
            HandleFrame(frame);
        }
    }

    protected Http2Frame ReadFrame() => Read(stream);

    protected void Send(Http2FrameType type, Http2Flags flags, int streamId, ReadOnlySpan<byte> payload) =>
        Write(stream, type, flags, streamId, payload);

    protected virtual void HandleFrame(Http2Frame frame)
    {
        if (frame.StreamId != ConnectionStreamId || frame.Flags != Http2Flags.None)
        {
            return;
        }

        switch (frame.Type)
        {
            case Http2FrameType.Settings when frame.Payload.Length % SettingsEntryLength == 0:
                ApplySettings(frame.Payload);
                Send(Http2FrameType.Settings, Http2Flags.Ack, ConnectionStreamId, []);
                break;

            case Http2FrameType.Ping when frame.Payload.Length == PingPayloadLength:
                Send(Http2FrameType.Ping, Http2Flags.Ack, ConnectionStreamId, frame.Payload);
                break;
        }
    }

    protected virtual void ApplySettings(ReadOnlySpan<byte> payload)
    {
        // This stage uses the default settings.
    }
}
