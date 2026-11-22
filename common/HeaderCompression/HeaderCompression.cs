using System.Text;

internal delegate byte[] HuffmanDecoder(ReadOnlySpan<byte> input);

// HPACK and QPACK share the prefixed integer and string literal formats.
internal static class HeaderCompression
{
    public static int ReadByte(Stream input) =>
        input.ReadByte() is var value && value >= 0
            ? value
            : throw new InvalidDataException("Truncated header block.", new EndOfStreamException());

    public static void WriteInteger(Stream output, int value, int bits, int flags = 0)
    {
        var mask = (1 << bits) - 1;
        output.WriteByte((byte)(flags | Math.Min(value, mask)));
        if (value < mask)
        {
            return;
        }

        var remaining = value - mask;
        while (remaining >= 128)
        {
            output.WriteByte((byte)((remaining & 127) | 128));
            remaining >>= 7;
        }

        output.WriteByte((byte)remaining);
    }

    public static void WriteText(Stream output, string value)
    {
        var bytes = Encoding.Latin1.GetBytes(value);
        WriteInteger(output, bytes.Length, 7);
        output.Write(bytes);
    }

    public static int ReadInteger(Stream input, int first, int bits)
    {
        var mask = (1 << bits) - 1;
        long value = first & mask;
        if (value < mask)
        {
            return (int)value;
        }

        for (var shift = 0; shift <= 28; shift += 7)
        {
            var next = ReadByte(input);
            value += (long)(next & 127) << shift;
            if (value > int.MaxValue)
            {
                break;
            }

            if ((next & 128) == 0)
            {
                return (int)value;
            }
        }

        throw new InvalidDataException("Header integer overflow.");
    }

    public static string ReadText(Stream input, HuffmanDecoder? decodeHuffman)
    {
        var first = ReadByte(input);
        var length = ReadInteger(input, first, 7);
        return ReadText(input, length, (first & 0x80) != 0, decodeHuffman);
    }

    public static string ReadText(Stream input, int length, bool huffman, HuffmanDecoder? decodeHuffman)
    {
        if (length > 65536)
        {
            throw new InvalidDataException("Invalid header string length.");
        }

        if (length > input.Length - input.Position)
        {
            throw new InvalidDataException("Truncated header string.", new EndOfStreamException());
        }

        var bytes = new byte[length];
        input.ReadExactly(bytes);
        if (huffman)
        {
            if (decodeHuffman is null)
            {
                throw new InvalidDataException("Huffman is not implemented yet.");
            }

            bytes = decodeHuffman(bytes);
        }

        return Encoding.Latin1.GetString(bytes);
    }
}
