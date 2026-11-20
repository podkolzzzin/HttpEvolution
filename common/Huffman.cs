// Canonical code lengths from RFC 7541 Appendix B (also used by QPACK).
static class Huffman
{
    static readonly byte[] Lengths =
    [
        13, 23, 28, 28, 28, 28, 28, 28, 28, 24, 30, 28, 28, 30, 28, 28, 28, 28, 28, 28, 28, 28, 30, 28,
        28, 28, 28, 28, 28, 28, 28, 28, 6, 10, 10, 12, 13, 6, 8, 11, 10, 10, 8, 11, 8, 6, 6, 6,
        5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 7, 8, 15, 6, 12, 10, 13, 6, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 7, 8, 13, 19, 13, 14, 6,
        15, 5, 6, 5, 6, 5, 6, 6, 6, 5, 7, 7, 6, 6, 6, 5, 6, 7, 6, 5, 5, 6, 7, 7,
        7, 7, 7, 15, 11, 14, 13, 28, 20, 22, 20, 20, 22, 22, 22, 23, 22, 23, 23, 23, 23, 23, 24, 23,
        24, 24, 22, 23, 24, 23, 23, 23, 23, 21, 22, 23, 22, 23, 23, 24, 22, 21, 20, 22, 22, 23, 23, 21,
        23, 22, 22, 24, 21, 22, 23, 23, 21, 21, 22, 21, 23, 22, 23, 23, 20, 22, 22, 22, 23, 22, 22, 23,
        26, 26, 20, 19, 22, 23, 22, 25, 26, 26, 26, 27, 27, 26, 24, 25, 19, 21, 26, 27, 27, 26, 27, 24,
        21, 21, 26, 26, 28, 27, 27, 27, 20, 24, 20, 21, 22, 21, 21, 23, 22, 22, 25, 25, 24, 24, 26, 23,
        26, 27, 26, 26, 27, 27, 27, 27, 27, 28, 27, 27, 27, 27, 27, 26, 30,
    ];
    static readonly Dictionary<(int Bits, uint Code), int> Codes = Build();

    static Dictionary<(int, uint), int> Build()
    {
        var codes = new Dictionary<(int, uint), int>();
        uint code = 0;
        var previous = 0;
        foreach (var symbol in Enumerable.Range(0, Lengths.Length).OrderBy(i => Lengths[i]).ThenBy(i => i))
        {
            var bits = Lengths[symbol];
            code <<= bits - previous;
            codes.Add((bits, code++), symbol);
            previous = bits;
        }
        return codes;
    }

    public static byte[] Decode(ReadOnlySpan<byte> input)
    {
        using var output = new MemoryStream();
        uint code = 0;
        var bits = 0;
        foreach (var octet in input)
            for (var shift = 7; shift >= 0; shift--)
            {
                code = (code << 1) | (uint)((octet >> shift) & 1);
                bits++;
                if (Codes.TryGetValue((bits, code), out var symbol))
                {
                    if (symbol == 256) throw new InvalidDataException("Huffman EOS inside a string.");
                    output.WriteByte((byte)symbol);
                    code = 0; bits = 0;
                }
                else if (bits >= 30) throw new InvalidDataException("Invalid Huffman code.");
            }
        if (bits > 7 || code != (1u << bits) - 1) throw new InvalidDataException("Invalid Huffman padding.");
        return output.ToArray();
    }
}
