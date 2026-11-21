using System.Buffers.Binary;
using System.Text;

internal class Http2StreamConnection(Stream stream) : Http2Connection(stream)
{
    private const int PriorityFieldLength = 5;

    // Incoming streams share this connection's decoder and dynamic table.
    private readonly Hpack hpack = new(Huffman.Decode);

    protected override void HandleFrame(Http2Frame frame)
    {
        if (IsRequestHeaders(frame))
        {
            HandleHeaders(frame);
            return;
        }

        HandleOtherFrame(frame);
    }

    protected virtual void HandleOtherFrame(Http2Frame frame) => base.HandleFrame(frame);

    private bool IsRequestHeaders(Http2Frame frame) =>
        frame.Type == Http2FrameType.Headers &&
        frame.StreamId > Http2Frames.ConnectionStreamId &&
        CanReadHeaders(frame);

    protected virtual bool CanReadHeaders(Http2Frame frame) =>
        frame.HasFlags(Http2Flags.EndHeaders | Http2Flags.EndStream);

    private void HandleHeaders(Http2Frame frame)
    {
        OpenStream(frame.StreamId);
        var fields = hpack.Decode(ReadHeaderBlock(frame));
        if (!frame.HasFlags(Http2Flags.EndStream) || !CanRespond())
        {
            RefuseStream(frame.StreamId);
            return;
        }

        var method = fields.Find(field => field.Name == ":method")?.Value;
        var path = fields.Find(field => field.Name == ":path")?.Value;
        var status = ResponseStatus(method, path);
        var body = Encoding.ASCII.GetBytes(status == "200" ? "<h1>Hello, HTTP!</h1>\n" : $"<h1>{status}</h1>\n");
        var headers = EncodeResponseHeaders(
            new(":status", status),
            new("content-type", "text/html"),
            new("content-length", body.Length.ToString())
        );

        var head = method == "HEAD";
        var flags = Http2Flags.EndHeaders | (head ? Http2Flags.EndStream : Http2Flags.None);
        Send(Http2FrameType.Headers, flags, frame.StreamId, headers);
        if (!head)
        {
            SendBody(frame.StreamId, body);
        }

        AfterResponse();
    }

    protected virtual void OpenStream(int streamId)
    {
    }

    protected virtual bool CanRespond() => true;

    protected virtual byte[] ReadHeaderBlock(Http2Frame frame) => HeaderFragment(frame).ToArray();

    protected virtual void SendBody(int streamId, byte[] body) =>
        Send(Http2FrameType.Data, Http2Flags.EndStream, streamId, body);

    protected virtual void AfterResponse()
    {
    }

    protected static ReadOnlySpan<byte> HeaderFragment(Http2Frame frame)
    {
        var payload = frame.Payload.AsSpan();
        var padded = frame.HasFlags(Http2Flags.Padded);
        var start = padded ? sizeof(byte) : 0;
        var padding = padded && !payload.IsEmpty ? payload[0] : 0;
        if (frame.HasFlags(Http2Flags.Priority))
        {
            start += PriorityFieldLength;
        }

        if (start + padding > payload.Length)
        {
            throw new InvalidDataException("Invalid HEADERS padding.");
        }

        return payload[start..(payload.Length - padding)];
    }

    private void RefuseStream(int streamId)
    {
        const uint refusedStream = 0x07;
        Span<byte> error = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(error, refusedStream);
        Send(Http2FrameType.ResetStream, Http2Flags.None, streamId, error);
    }

    private static string ResponseStatus(string? method, string? path)
    {
        if (method is null || path is null)
        {
            return "400";
        }

        if (method is not ("GET" or "HEAD"))
        {
            return "405";
        }

        return path is "/" or "/chunks" ? "200" : "404";
    }

    private static byte[] EncodeResponseHeaders(params HeaderField[] fields)
    {
        using var output = new MemoryStream();
        foreach (var field in fields)
        {
            output.WriteByte(0); // Literal field without indexing, with a literal name.
            HeaderCompression.WriteText(output, field.Name);
            HeaderCompression.WriteText(output, field.Value);
        }

        return output.ToArray();
    }
}
