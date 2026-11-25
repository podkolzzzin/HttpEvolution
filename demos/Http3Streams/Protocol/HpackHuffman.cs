using System.Text;

namespace Http3Streams.Protocol;

// QPACK uses the canonical HPACK Huffman code from RFC 7541 Appendix B.
internal static class HpackHuffman
{
    private static readonly byte[] Lengths =
    [
        13, 23, 28, 28, 28, 28, 28, 28, 28, 24, 30, 28, 28, 30, 28, 28,
        28, 28, 28, 28, 28, 28, 30, 28, 28, 28, 28, 28, 28, 28, 28, 28,
        6, 10, 10, 12, 13, 6, 8, 11, 10, 10, 8, 11, 8, 6, 6, 6,
        5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 7, 8, 15, 6, 12, 10,
        13, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 8, 7, 8, 13, 19, 13, 14, 6,
        15, 5, 6, 5, 6, 5, 6, 6, 6, 5, 7, 7, 6, 6, 6, 5,
        6, 7, 6, 5, 5, 6, 7, 7, 7, 7, 7, 15, 11, 14, 13, 28,
        20, 22, 20, 20, 22, 22, 22, 23, 22, 23, 23, 23, 23, 23, 24, 23,
        24, 24, 22, 23, 24, 23, 23, 23, 23, 21, 22, 23, 22, 23, 23, 24,
        22, 21, 20, 22, 22, 23, 23, 21, 23, 22, 22, 24, 21, 22, 23, 23,
        21, 21, 22, 21, 23, 22, 23, 23, 20, 22, 22, 22, 23, 22, 22, 23,
        26, 26, 20, 19, 22, 23, 22, 25, 26, 26, 26, 27, 27, 26, 24, 25,
        19, 21, 26, 27, 27, 26, 27, 24, 21, 21, 26, 26, 28, 27, 27, 27,
        20, 24, 20, 21, 22, 21, 21, 23, 22, 22, 25, 25, 24, 24, 26, 23,
        26, 27, 26, 26, 27, 27, 27, 27, 27, 28, 27, 27, 27, 27, 27, 26,
        30
    ];

    // A leading 1 in each key preserves leading zeroes in a codeword.
    private static readonly Dictionary<uint, int> Symbols = BuildTable();

    private static Dictionary<uint, int> BuildTable()
    {
        var symbols = new Dictionary<uint, int>();
        uint code = 0;
        int previousLength = 0;
        foreach (int symbol in Enumerable.Range(0, Lengths.Length).OrderBy(symbol => Lengths[symbol]))
        {
            int length = Lengths[symbol];
            code <<= length - previousLength;
            symbols[(1u << length) | code] = symbol;
            code++;
            previousLength = length;
        }

        return symbols;
    }

    public static string Decode(ReadOnlySpan<byte> encoded)
    {
        var result = new StringBuilder();
        uint prefix = 1;
        foreach (byte octet in encoded)
        {
            for (int bit = 7; bit >= 0; bit--)
            {
                prefix = (prefix << 1) | (uint)((octet >> bit) & 1);
                if (Symbols.TryGetValue(prefix, out int symbol))
                {
                    if (symbol == 256)
                    {
                        throw new InvalidDataException("Unexpected end-of-string symbol in a Huffman string.");
                    }

                    result.Append((char)symbol);
                    prefix = 1;
                }
            }
        }

        // Up to seven trailing one bits may pad the last octet.
        if (prefix > 255 || (prefix & (prefix + 1)) != 0)
        {
            throw new InvalidDataException("Invalid Huffman padding.");
        }

        return result.ToString();
    }
}
