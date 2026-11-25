namespace Http3Streams.Quic;

internal sealed class TransportParameters
{
    public const int StreamWindow = 256 * 1024;
    public const int ConnectionWindow = 8 * 1024 * 1024;
    public const int StreamCount = 64;
    private readonly Dictionary<ulong, byte[]> values = [];

    public TransportParameters(byte[] encoded, byte[] peerSourceId, byte[]? originalId)
    {
        var input = new QuicData(encoded);
        while (input.Left > 0)
        {
            if (!values.TryAdd(input.Int(), input.Vector()))
            {
                throw new InvalidDataException("Duplicate QUIC transport parameter.");
            }
        }

        RequireId(15, peerSourceId);
        if (originalId is not null)
        {
            RequireId(0, originalId);
        }
        else if (new ulong[] { 0, 2, 13, 16 }.Any(values.ContainsKey))
        {
            throw new InvalidDataException("Received a server-only transport parameter from the client.");
        }

        if (Number(3, 65527) < 1200 || Number(8) > (1UL << 60) || Number(9) > (1UL << 60)
            || Number(10, 3) > 20 || Number(11, 25) >= 16384 || Number(14, 2) < 2)
        {
            throw new InvalidDataException("Invalid QUIC transport limit.");
        }
    }

    private void RequireId(ulong key, byte[] expected)
    {
        if (!values.TryGetValue(key, out var id) || !id.AsSpan().SequenceEqual(expected))
        {
            throw new InvalidDataException("QUIC transport connection ID mismatch.");
        }
    }

    public ulong Number(ulong key, ulong fallback = 0)
    {
        if (!values.TryGetValue(key, out var value))
        {
            return fallback;
        }

        var input = new QuicData(value);
        var number = input.Int();
        return input.Left == 0 ? number : throw new InvalidDataException("Invalid numeric transport parameter.");
    }

    public static byte[] Encode(byte[] sourceId, byte[]? originalId)
    {
        using var output = new MemoryStream();
        if (originalId is not null)
        {
            output.Write(QuicData.Pair(0, originalId));
        }

        output.Write(QuicData.Pair(15, sourceId));
        output.Write(QuicData.Pair(12, [])); // disable_active_migration
        void Put(ulong id, ulong value) => output.Write(QuicData.Pair(id, QuicData.Int(value)));
        Put(1, 30000);
        Put(3, 1200);
        Put(4, ConnectionWindow);
        Put(5, StreamWindow);
        Put(6, StreamWindow);
        Put(7, StreamWindow);
        Put(8, StreamCount);
        Put(9, StreamCount);
        return output.ToArray();
    }
}
