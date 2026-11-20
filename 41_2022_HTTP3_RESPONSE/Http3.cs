using System.Text;

sealed class Http3
{
    sealed class State
    {
        public ulong? Type;
        public bool Settings;
        public bool Done;
        public bool HasBody;
        public List<(string Name, string Value)>? Headers;
    }

    readonly QuicStreams streams;
    readonly Dictionary<ulong, State> states = [];
    readonly HashSet<ulong> critical = [];
    public Http3(QuicStreams streams)
    {
        this.streams = streams;
        byte[] settings = [1, 0, 7, 0, 6, .. QuicData.Int(65536)]; // QPACK capacity/blocked=0, max fields=64KiB.
        streams.Send(3, [0, .. Frame(4, settings)], false); // Server control stream stays open.
    }

    static byte[] Frame(ulong type, byte[] data) => [.. QuicData.Int(type), .. QuicData.Vector(data)];
    static bool ReadFrame(QuicBuffer buffer, out ulong type, out byte[] data)
    {
        type = 0;
        data = [];
        var input = new QuicData(buffer.Peek());
        ulong length;
        try
        {
            type = input.Int();
            length = input.Int();
        }
        catch (InvalidDataException)
        {
            return false;
        }

        if (length > 65536)
        {
            throw new InvalidDataException("HTTP/3 frame too large.");
        }

        if (length > (ulong)input.Left)
        {
            return false;
        }

        data = input.Take((int)length);
        buffer.Consume(input.Offset);
        return true;
    }

    public void Process()
    {
        foreach (var (id, buffer) in streams.Input)
        {
            if (!states.TryGetValue(id, out var state))
            {
                states[id] = state = new();
            }

            if (streams.Reset.Contains(id))
            {
                if (state.Type is 0 or 2 or 3)
                {
                    throw new InvalidDataException("Critical HTTP/3 stream reset.");
                }

                state.Done = true;
            }

            if (state.Done)
            {
                continue;
            }

            if ((id & 2) != 0)
            {
                if (state.Type is null)
                {
                    var input = new QuicData(buffer.Peek());
                    ulong type;
                    try
                    {
                        type = input.Int();
                    }
                    catch (InvalidDataException)
                    {
                        if (buffer.Finished)
                        {
                            throw new InvalidDataException("Truncated stream type.");
                        }

                        continue;
                    }

                    buffer.Consume(input.Offset);
                    state.Type = type;
                    if (type is 0 or 2 or 3)
                    {
                        if (!critical.Add(type))
                        {
                            throw new InvalidDataException("Duplicate critical HTTP/3 stream.");
                        }
                    }

                    if (type == 1)
                    {
                        throw new InvalidDataException("Client push stream.");
                    }
                }

                if (state.Type is 0 or 2 or 3 && buffer.Finished)
                {
                    throw new InvalidDataException("Critical HTTP/3 stream closed.");
                }

                if (state.Type == 2)
                {
                    if (buffer.Peek().Any(value => value != 0x20))
                    {
                        throw new InvalidDataException("QPACK encoder exceeds zero capacity.");
                    }

                    buffer.Consume(buffer.Available);
                    continue;
                }

                if (state.Type != 0)
                {
                    buffer.Consume(buffer.Available);
                    continue;
                }
            }

            while (ReadFrame(buffer, out var type, out var data))
            {
                if (type is 2 or 6 or 8 or 9)
                {
                    throw new InvalidDataException("HTTP/2 frame in HTTP/3.");
                }

                if (state.Type == 0)
                {
                    if (!state.Settings && type != 4)
                    {
                        throw new InvalidDataException("SETTINGS must be first.");
                    }

                    if (type == 4)
                    {
                        if (state.Settings)
                        {
                            throw new InvalidDataException("Repeated SETTINGS.");
                        }

                        var settings = new QuicData(data);
                        var ids = new HashSet<ulong>();
                        while (settings.Left > 0)
                        {
                            var setting = settings.Int();
                            settings.Int();
                            if (!ids.Add(setting) || setting is >= 2 and <= 5)
                            {
                                throw new InvalidDataException("Invalid HTTP/3 setting.");
                            }
                        }

                        state.Settings = true;
                    }

                    if (type is 0 or 1 or 5)
                    {
                        throw new InvalidDataException("Request frame on control stream.");
                    }
                }
                else if (type == 1)
                {
                    if (state.Headers is not null)
                    {
                        throw new InvalidDataException("Request trailers are outside this demo.");
                    }

                    state.Headers = Qpack.Decode(data);
                }
                else if (type == 0)
                {
                    if (state.Headers is null)
                    {
                        throw new InvalidDataException("DATA before HEADERS.");
                    }

                    state.HasBody |= data.Length != 0;
                }
                else if (type is 3 or 4 or 5 or 7 or 13)
                {
                    throw new InvalidDataException("Control frame on request stream.");
                }
            }

            if (buffer.Finished && buffer.Available != 0)
            {
                throw new InvalidDataException("Truncated HTTP/3 frame.");
            }

            if (state.Type is null && buffer.Finished)
            {
                Respond(id, state);
                state.Done = true;
            }
        }
    }

    void Respond(ulong id, State state)
    {
        var pseudo = new Dictionary<string, string>();
        bool regular = false;
        bool bad = state.HasBody;
        foreach (var (name, value) in state.Headers ?? [])
        {
            bad |= name.Length == 0 || name != name.ToLowerInvariant()
                || name.Any(c => c <= 32 || c >= 127)
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
        var status = bad ? "400" : method is not ("GET" or "HEAD") ? "405" : path is "/" or "/chunks" ? "200" : "404";
        var body = Encoding.ASCII.GetBytes(status == "200" ? "<h1>Hello, HTTP!</h1>\n" : $"<h1>{status}</h1>\n");
        var fields = new List<(string, string)>
        {
            (":status", status),
            ("content-type", "text/html"),
            ("content-length", body.Length.ToString())
        };
        if (status == "405")
        {
            fields.Add(("allow", "GET, HEAD"));
        }

        byte[] response = [.. Frame(1, Qpack.Encode(fields.ToArray())), .. (method == "HEAD" ? [] : Frame(0, body))];
        streams.Send(id, response, true);
    }
}
