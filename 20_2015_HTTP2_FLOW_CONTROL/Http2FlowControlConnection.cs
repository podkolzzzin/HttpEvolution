using System.Buffers.Binary;
using static Http2Frames;

internal class Http2FlowControlConnection(Stream stream) : Http2StreamConnection(stream)
{
    private const int DefaultWindow = 65_535;
    private const int MaxWindow = 0x7fff_ffff;
    private const int MaxPendingStreams = 128;
    private const ushort InitialWindowSizeSetting = 0x04;
    private const int ClientStreamBit = 1;

    private sealed class PendingResponse(byte[] body, long window)
    {
        public byte[] Body { get; } = body;
        public int Offset
        {
            get; set;
        }
        public long Window { get; set; } = window;
    }

    private readonly Dictionary<int, PendingResponse> pending = [];
    private long initialStreamWindow = DefaultWindow;
    private long connectionWindow = DefaultWindow;
    private int lastClientStreamId;

    protected override void HandleOtherFrame(Http2Frame frame)
    {
        switch (frame.Type)
        {
            case Http2FrameType.ResetStream when frame.Payload.Length == sizeof(uint):
                pending.Remove(frame.StreamId);
                break;

            case Http2FrameType.WindowUpdate when frame.Payload.Length == sizeof(uint):
                UpdateWindow(frame);
                break;

            default:
                base.HandleOtherFrame(frame);
                break;
        }

        FlushResponses();
    }

    protected override void ApplySettings(ReadOnlySpan<byte> payload)
    {
        for (var offset = 0; offset < payload.Length; offset += SettingsEntryLength)
        {
            var entry = payload[offset..];
            if (BinaryPrimitives.ReadUInt16BigEndian(entry) != InitialWindowSizeSetting)
            {
                continue;
            }

            var window = BinaryPrimitives.ReadUInt32BigEndian(entry[sizeof(ushort)..]);
            if (window > MaxWindow)
            {
                throw new InvalidDataException("Invalid initial stream window.");
            }

            var change = window - initialStreamWindow;
            foreach (var response in pending.Values)
            {
                response.Window += change;
            }

            initialStreamWindow = window;
        }
    }

    protected override void OpenStream(int streamId)
    {
        if ((streamId & ClientStreamBit) == 0 || streamId <= lastClientStreamId)
        {
            throw new InvalidDataException("Invalid new client stream.");
        }

        lastClientStreamId = streamId;
    }

    protected override bool CanRespond() => pending.Count < MaxPendingStreams;

    protected override void SendBody(int streamId, byte[] body) =>
        pending.Add(streamId, new(body, initialStreamWindow));

    protected override void AfterResponse() => FlushResponses();

    private void UpdateWindow(Http2Frame frame)
    {
        var increment = BinaryPrimitives.ReadUInt32BigEndian(frame.Payload) & MaxWindow;
        if (increment == 0)
        {
            throw new InvalidDataException("Zero window increment.");
        }

        if (frame.StreamId == ConnectionStreamId)
        {
            connectionWindow += increment;
        }
        else if (pending.TryGetValue(frame.StreamId, out var response))
        {
            response.Window += increment;
        }

        if (connectionWindow > MaxWindow || pending.Values.Any(response => response.Window > MaxWindow))
        {
            throw new InvalidDataException("Flow-control window overflow.");
        }
    }

    private void FlushResponses()
    {
        foreach (var (streamId, response) in pending)
        {
            var available = Math.Min(response.Window, connectionWindow);
            var length = (int)Math.Min(response.Body.Length - response.Offset, Math.Min(MaxFramePayload, available));
            if (length <= 0)
            {
                continue;
            }

            var done = response.Offset + length == response.Body.Length;
            Send(Http2FrameType.Data, done ? Http2Flags.EndStream : Http2Flags.None,
                streamId, response.Body.AsSpan(response.Offset, length));

            connectionWindow -= length;
            if (done)
            {
                pending.Remove(streamId); // Removing a key keeps the dictionary enumerator valid.
            }
            else
            {
                response.Offset += length;
                response.Window -= length;
            }
        }
    }
}
