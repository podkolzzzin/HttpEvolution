sealed class QuicParameters(byte[] bytes)
{
    readonly Dictionary<ulong, byte[]> values = Parse(bytes);
    static Dictionary<ulong, byte[]> Parse(byte[] bytes)
    {
        var input = new QuicData(bytes); var result = new Dictionary<ulong, byte[]>();
        while (input.Left > 0)
            if (!result.TryAdd(input.Int(), input.Vector())) throw new InvalidDataException("Duplicate transport parameter.");
        return result;
    }
    public ulong Value(ulong id, ulong fallback = 0)
    {
        if (!values.TryGetValue(id, out var bytes)) return fallback;
        var input = new QuicData(bytes); var value = input.Int();
        if (input.Left != 0) throw new InvalidDataException("Invalid numeric transport parameter.");
        return value;
    }
    public void ValidateClient(byte[] source)
    {
        if (!values.TryGetValue(15, out var id) || !id.AsSpan().SequenceEqual(source)) throw new InvalidDataException("Client source connection ID mismatch.");
        if (new ulong[] { 0, 2, 13, 16 }.Any(values.ContainsKey)) throw new InvalidDataException("Server-only client parameter.");
        if (Value(3, 65527) < 1200 || Value(8) > (1UL << 60) || Value(9) > (1UL << 60) || Value(10, 3) > 20 || Value(11, 25) >= 16384)
            throw new InvalidDataException("Invalid QUIC transport limit.");
    }
    public static byte[] Server(byte[] original, byte[] source)
    {
        using var output = new MemoryStream();
        output.Write(QuicData.Pair(0, original)); output.Write(QuicData.Pair(15, source));
        foreach (var (id, value) in new (ulong, ulong)[] { (1, 10000), (3, 1200), (4, 1048576), (5, 65536), (6, 65536), (7, 65536), (8, 16), (9, 16) })
            output.Write(QuicData.Pair(id, QuicData.Int(value)));
        output.Write(QuicData.Pair(12, [])); // Disable active migration for the loopback demo.
        return output.ToArray();
    }
}
