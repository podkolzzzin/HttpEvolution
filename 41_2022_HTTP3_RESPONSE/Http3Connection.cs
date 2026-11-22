using System.Net;
using System.Text;
using static HeaderCompression;

internal sealed class Http3Connection
{
    private const ulong ServerControlStreamId = 0x03;
    private const ulong ServerQpackDecoderStreamId = 0x07;
    private const ulong UnidirectionalStreamBit = 0x02;
    private const int MaxFramePayload = 65_536;
    private const char AsciiDelete = '\u007f';

    private const byte QpackMaxTableCapacitySetting = 0x01;
    private const byte MaxFieldSectionSizeSetting = 0x06;
    private const byte QpackBlockedStreamsSetting = 0x07;
    private const byte FirstReservedHttp2Setting = 0x02;
    private const byte LastReservedHttp2Setting = 0x05;

    private const int QpackMaxTableCapacity = 4096;
    private const byte QpackBlockedStreams = 1;
    private const int SectionAcknowledgment = 0x80;
    private const int StreamCancellation = 0x40;

    private enum StreamType : ulong
    {
        Control = 0x00,
        Push = 0x01,
        QpackEncoder = 0x02,
        QpackDecoder = 0x03
    }

    private enum FrameType : ulong
    {
        Data = 0x00,
        Headers = 0x01,
        ReservedHttp2Priority = 0x02,
        CancelPush = 0x03,
        Settings = 0x04,
        PushPromise = 0x05,
        ReservedHttp2Ping = 0x06,
        GoAway = 0x07,
        ReservedHttp2WindowUpdate = 0x08,
        ReservedHttp2Continuation = 0x09,
        MaxPushId = 0x0d
    }

    private sealed class State
    {
        public StreamType? Type;
        public bool Settings;
        public bool Done;
        public bool HasBody;
        public List<HeaderField>? Headers;
        public Qpack.FieldSection? PendingHeaders;
    }

    private readonly QuicStreams streams;
    private readonly Dictionary<ulong, State> states = [];
    private readonly HashSet<StreamType> critical = [];
    private readonly QpackDynamicTable qpack = new(QpackMaxTableCapacity);
    private bool decoderStreamOpened;

    public Http3Connection(QuicStreams streams)
    {
        this.streams = streams;
        byte[] settings =
        [
            QpackMaxTableCapacitySetting, .. QuicData.Int(QpackMaxTableCapacity),
            QpackBlockedStreamsSetting, QpackBlockedStreams,
            MaxFieldSectionSizeSetting, .. QuicData.Int(Qpack.MaxFieldSectionSize)
        ];
        streams.Send(ServerControlStreamId, [(byte)StreamType.Control, .. Frame(FrameType.Settings, settings)], fin: false);
    }

    private static bool IsCriticalStream(StreamType? type) =>
        type is StreamType.Control or StreamType.QpackEncoder or StreamType.QpackDecoder;

    private static byte[] Frame(FrameType type, byte[] data) => [.. QuicData.Int((ulong)type), .. QuicData.Vector(data)];
    private static bool ReadFrame(QuicBuffer buffer, out FrameType type, out byte[] data)
    {
        type = default;
        data = [];
        var input = new QuicData(buffer.Peek());
        ulong length;
        try
        {
            type = (FrameType)input.Int();
            length = input.Int();
        }
        catch (InvalidDataException)
        {
            return false;
        }

        if (length > MaxFramePayload)
        {
            throw new InvalidDataException("HTTP/3 frame too large.");
        }

        if (length > (ulong)input.Left)
        {
            return false;
        }

        data = input.Take((int)length);
        buffer.Consume(input.Offset);
        if (type is FrameType.ReservedHttp2Priority or FrameType.ReservedHttp2Ping
            or FrameType.ReservedHttp2WindowUpdate or FrameType.ReservedHttp2Continuation)
        {
            throw new InvalidDataException("HTTP/2 frame in HTTP/3.");
        }

        return true;
    }

    public void Process()
    {
        // Apply encoder instructions before retrying blocked request streams.
        foreach (var (id, buffer) in streams.Input)
        {
            if (IsUnidirectional(id) && ActiveState(id) is { } state)
            {
                ProcessUnidirectional(buffer, state);
            }
        }

        foreach (var (id, buffer) in streams.Input)
        {
            if (!IsUnidirectional(id) && ActiveState(id) is { } state)
            {
                ProcessRequest(id, buffer, state);
            }
        }
    }

    private static bool IsUnidirectional(ulong id) => (id & UnidirectionalStreamBit) != 0;

    private State? ActiveState(ulong id)
    {
        if (!states.TryGetValue(id, out var state))
        {
            states[id] = state = new();
        }

        if (!state.Done && streams.Reset.Contains(id))
        {
            if (IsCriticalStream(state.Type))
            {
                throw new InvalidDataException("Critical HTTP/3 stream reset.");
            }

            if (!IsUnidirectional(id))
            {
                SendDecoderInstruction((int)id, 6, StreamCancellation);
            }

            state.PendingHeaders = null;
            state.Done = true;
        }

        return state.Done ? null : state;
    }

    private bool ReadStreamType(QuicBuffer buffer, State state)
    {
        var input = new QuicData(buffer.Peek());
        StreamType type;
        try
        {
            type = (StreamType)input.Int();
        }
        catch (InvalidDataException)
        {
            if (buffer.Finished)
            {
                throw new InvalidDataException("Truncated stream type.");
            }

            return false;
        }

        buffer.Consume(input.Offset);
        state.Type = type;
        if (IsCriticalStream(type) && !critical.Add(type))
        {
            throw new InvalidDataException("Duplicate critical HTTP/3 stream.");
        }

        if (type == StreamType.Push)
        {
            throw new InvalidDataException("Client push stream.");
        }

        return true;
    }

    private void ProcessUnidirectional(QuicBuffer buffer, State state)
    {
        if (state.Type is null && !ReadStreamType(buffer, state))
        {
            return;
        }

        if (IsCriticalStream(state.Type) && buffer.Finished)
        {
            throw new InvalidDataException("Critical HTTP/3 stream closed.");
        }

        switch (state.Type)
        {
            case StreamType.QpackEncoder:
                ProcessEncoderStream(buffer);
                break;
            case StreamType.Control:
                ProcessControlStream(buffer, state);
                break;
            default:
                buffer.Consume(buffer.Available);
                break;
        }
    }

    private void ProcessEncoderStream(QuicBuffer buffer)
    {
        if (buffer.Available == 0)
        {
            return;
        }

        var before = qpack.InsertCount;
        buffer.Consume(Qpack.ReadEncoderInstructions(buffer.Peek(), qpack));
        var inserted = qpack.InsertCount - before;
        if (inserted > 0)
        {
            SendDecoderInstruction(inserted, 6, flags: 0); // Insert Count Increment.
        }
    }

    private void ProcessControlStream(QuicBuffer buffer, State state)
    {
        while (ReadFrame(buffer, out var type, out var data))
        {
            if (!state.Settings && type != FrameType.Settings)
            {
                throw new InvalidDataException("SETTINGS must be first.");
            }

            if (type == FrameType.Settings)
            {
                if (state.Settings)
                {
                    throw new InvalidDataException("Repeated SETTINGS.");
                }

                ValidateSettings(data);
                state.Settings = true;
            }

            if (type is FrameType.Data or FrameType.Headers or FrameType.PushPromise)
            {
                throw new InvalidDataException("Request frame on control stream.");
            }
        }
    }

    private static void ValidateSettings(byte[] data)
    {
        var settings = new QuicData(data);
        var ids = new HashSet<ulong>();
        while (settings.Left > 0)
        {
            var setting = settings.Int();
            settings.Int();
            if (!ids.Add(setting) || setting is >= FirstReservedHttp2Setting and <= LastReservedHttp2Setting)
            {
                throw new InvalidDataException("Invalid HTTP/3 setting.");
            }
        }
    }

    private void ProcessRequest(ulong id, QuicBuffer buffer, State state)
    {
        if (!TryDecodeHeaders(id, state))
        {
            return;
        }

        while (ReadFrame(buffer, out var type, out var data))
        {
            if (type == FrameType.Headers)
            {
                if (!ReceiveHeaders(id, state, data))
                {
                    return;
                }
            }
            else
            {
                ProcessRequestFrame(type, data, state);
            }
        }

        if (buffer.Finished)
        {
            if (buffer.Available != 0)
            {
                throw new InvalidDataException("Truncated HTTP/3 frame.");
            }

            Respond(id, state);
            state.Done = true;
        }
    }

    private bool ReceiveHeaders(ulong id, State state, byte[] data)
    {
        if (state.Headers is not null)
        {
            throw new InvalidDataException("Request trailers are outside this demo.");
        }

        state.PendingHeaders = Qpack.ReadFieldSection(data, qpack);
        if (TryDecodeHeaders(id, state))
        {
            return true;
        }

        if (CountBlockedRequests() > QpackBlockedStreams)
        {
            throw new InvalidDataException("QPACK blocked stream limit exceeded.");
        }

        return false;
    }

    private int CountBlockedRequests() => states.Count(entry =>
        entry.Value.PendingHeaders is { } section &&
        section.RequiredInsertCount > qpack.InsertCount &&
        !streams.Reset.Contains(entry.Key));

    private bool TryDecodeHeaders(ulong id, State state)
    {
        if (state.PendingHeaders is not { } section)
        {
            return true;
        }

        if (section.RequiredInsertCount > qpack.InsertCount)
        {
            return false;
        }

        state.Headers = Qpack.Decode(section, qpack);
        state.PendingHeaders = null;
        if (section.RequiredInsertCount > 0)
        {
            SendDecoderInstruction((int)id, 7, SectionAcknowledgment);
        }

        return true;
    }

    private static void ProcessRequestFrame(FrameType type, byte[] data, State state)
    {
        if (type == FrameType.Data)
        {
            if (state.Headers is null)
            {
                throw new InvalidDataException("DATA before HEADERS.");
            }

            state.HasBody |= data.Length != 0;
        }
        else if (type is FrameType.CancelPush or FrameType.Settings or FrameType.PushPromise
            or FrameType.GoAway or FrameType.MaxPushId)
        {
            throw new InvalidDataException("Control frame on request stream.");
        }
    }

    private void SendDecoderInstruction(int value, int prefixBits, int flags)
    {
        using var output = new MemoryStream();
        if (!decoderStreamOpened)
        {
            output.WriteByte((byte)StreamType.QpackDecoder);
            decoderStreamOpened = true;
        }

        WriteInteger(output, value, prefixBits, flags);
        streams.Send(ServerQpackDecoderStreamId, output.ToArray(), fin: false);
    }

    private void Respond(ulong id, State state)
    {
        var pseudo = new Dictionary<string, string>();
        bool regular = false;
        bool bad = state.HasBody;
        foreach (var (name, value) in state.Headers ?? [])
        {
            bad |= name.Length == 0 || name != name.ToLowerInvariant()
                || name.Any(c => c is <= ' ' or >= AsciiDelete)
                || value.Any(c => c is '\r' or '\n' or '\0');
            if (name.StartsWith(':'))
            {
                bad |= regular || name is not (":method" or ":scheme" or ":authority" or ":path");
                bad |= !pseudo.TryAdd(name, value);
            }
            else
            {
                regular = true;
                bad |= name is "connection" or "proxy-connection" or "keep-alive" or "transfer-encoding" or "upgrade";
                bad |= name == "te" && value != "trailers";
                bad |= name == "content-length" && value != "0";
            }
        }

        var method = pseudo.GetValueOrDefault(":method");
        var path = pseudo.GetValueOrDefault(":path");
        bad |= pseudo.GetValueOrDefault(":scheme") != "https"
            || string.IsNullOrEmpty(pseudo.GetValueOrDefault(":authority"))
            || string.IsNullOrEmpty(path)
            || method is null;
        var status = bad ? HttpStatusCode.BadRequest
            : method is not ("GET" or "HEAD") ? HttpStatusCode.MethodNotAllowed
            : path is "/" or "/chunks" ? HttpStatusCode.OK
            : HttpStatusCode.NotFound;
        var statusCode = status.ToString("D");
        var body = Encoding.ASCII.GetBytes(status == HttpStatusCode.OK ? "<h1>Hello, HTTP!</h1>\n" : $"<h1>{statusCode}</h1>\n");
        var fields = new List<HeaderField>
        {
            new(":status", statusCode),
            new("content-type", "text/html"),
            new("content-length", body.Length.ToString())
        };
        if (status == HttpStatusCode.MethodNotAllowed)
        {
            fields.Add(new("allow", "GET, HEAD"));
        }

        byte[] response = [.. Frame(FrameType.Headers, Qpack.Encode(fields)), .. (method == "HEAD" ? [] : Frame(FrameType.Data, body))];
        streams.Send(id, response, fin: true);
    }
}
