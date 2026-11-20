sealed class QuicStreams(QuicParameters peer)
{
    sealed class Output(ulong limit)
    {
        public byte[] Bytes = [];
        public ulong Offset;
        public ulong Limit = limit;
        public bool Fin;
        public bool Ended;
    }

    readonly Dictionary<ulong, Output> output = [];
    readonly Dictionary<ulong, ulong> ends = [];
    ulong received;
    ulong sent;
    public Dictionary<ulong, QuicBuffer> Input { get; } = [];
    public HashSet<ulong> Reset { get; } = [];

    public ulong DataLimit = peer.Value(4);
    public ulong UniStreams = peer.Value(9);
    public void Receive(ulong id, ulong offset, byte[] bytes, bool fin)
    {
        if ((id & 3) is not (0 or 2) || (id >> 2) >= 16)
        {
            throw new InvalidDataException("Invalid client stream ID.");
        }

        if (offset > 65536 || (ulong)bytes.Length > 65536 - offset)
        {
            throw new InvalidDataException("Receive stream window exceeded.");
        }

        var end = offset + (ulong)bytes.Length;
        var previous = ends.GetValueOrDefault(id);
        var increment = end > previous ? end - previous : 0;
        if (received + increment > 1048576)
        {
            throw new InvalidDataException("Receive connection window exceeded.");
        }

        if (!Input.TryGetValue(id, out var buffer))
        {
            Input[id] = buffer = new();
        }

        buffer.Add(offset, bytes, fin);
        received += increment;
        ends[id] = Math.Max(end, previous);
    }

    public void Send(ulong id, byte[] bytes, bool fin)
    {
        if ((id & 3) is not (0 or 3))
        {
            throw new InvalidDataException("Invalid server send direction.");
        }

        if (!output.TryGetValue(id, out var stream))
        {
            output[id] = stream = new(peer.Value((id & 2) != 0 ? 7UL : 5UL));
        }

        if (stream.Fin)
        {
            throw new InvalidDataException("Write after FIN.");
        }

        stream.Bytes = [.. stream.Bytes, .. bytes];
        stream.Fin = fin;
    }

    public void Grant(ulong id, ulong limit)
    {
        if ((id & 3) is not (0 or 3))
        {
            throw new InvalidDataException("MAX_STREAM_DATA on a receive-only stream.");
        }

        if (!output.TryGetValue(id, out var stream))
        {
            output[id] = stream = new(peer.Value((id & 2) != 0 ? 7UL : 5UL));
        }

        stream.Limit = Math.Max(stream.Limit, limit);
    }

    public IEnumerable<byte[]> Frames()
    {
        foreach (var (id, stream) in output)
        {
            if ((id & 2) != 0 && (id >> 2) >= UniStreams)
            {
                continue;
            }

            while (!stream.Ended)
            {
                var remaining = (ulong)stream.Bytes.Length - stream.Offset;
                var available = Math.Min(
                    stream.Limit > stream.Offset ? stream.Limit - stream.Offset : 0,
                    DataLimit > sent ? DataLimit - sent : 0
                );
                var length = (int)Math.Min(remaining, Math.Min(800, available));
                if (length == 0 && (remaining != 0 || !stream.Fin))
                {
                    break;
                }

                var final = stream.Fin && (ulong)length == remaining;
                byte[] frame = [
                    (byte)(final ? 15 : 14),
                    ..QuicData.Int(id),
                    ..QuicData.Int(stream.Offset),
                    ..QuicData.Vector(stream.Bytes[(int)stream.Offset..((int)stream.Offset + length)])
                ];
                stream.Offset += (ulong)length;
                sent += (ulong)length;
                stream.Ended = final;
                yield return frame;
            }
        }
    }
}
