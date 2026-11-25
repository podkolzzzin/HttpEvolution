namespace Http3Streams.Quic;

// Flow control counts the highest offset on each stream, including bytes received out of order.
internal sealed class StreamFlow(bool client)
{
    private sealed record Fragment(ulong Id, ulong Offset, byte[] Bytes, bool Fin)
    {
        public ulong End => Offset + (ulong)Bytes.Length;
    }

    private readonly Dictionary<ulong, ulong> received = [];
    private readonly Dictionary<ulong, ulong> sent = [];
    private readonly Dictionary<ulong, ulong> grants = [];
    private readonly List<Fragment> queued = [];
    private ulong totalReceived;
    private ulong totalSent;
    private ulong sendLimit;
    private ulong bidirectionalLimit;
    private ulong unidirectionalLimit;
    private TransportParameters? peer;

    private bool LocallyInitiated(ulong id) => (id & 1) == (client ? 0UL : 1UL);

    public void Configure(TransportParameters parameters)
    {
        peer = parameters;
        sendLimit = parameters.Number(4);
        bidirectionalLimit = parameters.Number(8);
        unidirectionalLimit = parameters.Number(9);
    }

    public void Receive(ulong id, ulong end)
    {
        if (!LocallyInitiated(id) && id / 4 >= TransportParameters.StreamCount
            || LocallyInitiated(id) && ((id & 2) != 0 || !sent.ContainsKey(id)))
        {
            throw new InvalidDataException("Peer used a stream outside its permitted range.");
        }

        var old = received.GetValueOrDefault(id);
        var extra = Math.Max(old, end) - old;
        if (end > TransportParameters.StreamWindow || totalReceived + extra > TransportParameters.ConnectionWindow)
        {
            throw new InvalidDataException("Peer exceeded the receive window.");
        }

        received[id] = Math.Max(old, end);
        totalReceived += extra;
    }

    public void Grant(ulong id, ulong limit) => grants[id] = Math.Max(grants.GetValueOrDefault(id), limit);
    public void GrantConnection(ulong limit) => sendLimit = Math.Max(sendLimit, limit);
    public void GrantStreams(bool uni, ulong limit)
    {
        if (uni)
        {
            unidirectionalLimit = Math.Max(unidirectionalLimit, limit);
        }
        else
        {
            bidirectionalLimit = Math.Max(bidirectionalLimit, limit);
        }
    }

    public ulong Stop(ulong id)
    {
        queued.RemoveAll(part => part.Id == id);
        return sent.GetValueOrDefault(id);
    }

    public void Add(long id, long offset, ReadOnlyMemory<byte> data, bool fin)
    {
        // Leave plenty of room in each 1200-byte datagram for headers, ACK and authentication tag.
        long fragments = Math.Max(1, (data.Length + 799L) / 800);
        if (queued.Count + fragments > 1024)
        {
            throw new InvalidDataException("Outgoing stream queue exceeds the demo limit.");
        }

        do
        {

            int take = Math.Min(800, data.Length);
            queued.Add(new((ulong)id, (ulong)offset, data[..take].ToArray(), fin && take == data.Length));
            offset += take;
            data = data[take..];
        }
        while (!data.IsEmpty);
    }

    public void Schedule(QuicRecovery recovery)
    {
        if (peer is null)
        {
            return;
        }

        for (var i = 0; i < queued.Count && recovery.HasCapacity;)
        {
            var part = queued[i];
            var uni = (part.Id & 2) != 0;
            var local = LocallyInitiated(part.Id);
            var streamLimit = Math.Max(grants.GetValueOrDefault(part.Id), peer.Number(uni ? 7UL : local ? 6UL : 5UL));
            var highest = sent.GetValueOrDefault(part.Id);
            var extra = Math.Max(part.End, highest) - highest;
            if (local && part.Id / 4 >= (uni ? unidirectionalLimit : bidirectionalLimit)
                || part.End > streamLimit || totalSent + extra > sendLimit)
            {
                i++;
                continue;
            }

            recovery.Add(2, [(byte)(part.Fin ? 0x0f : 0x0e), .. QuicData.Int(part.Id),
                .. QuicData.Int(part.Offset), .. QuicData.Vector(part.Bytes)]);
            totalSent += extra;
            sent[part.Id] = Math.Max(part.End, highest);
            queued.RemoveAt(i);
        }
    }
}
