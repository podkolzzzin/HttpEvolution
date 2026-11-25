using System.Globalization;
using System.Text;

namespace Http3Streams.Protocol;

// RFC 9204 §4.5 and Appendix A. Both peers use the default dynamic table capacity of zero.
internal static class StaticQpack
{
    private const int Authority = 0;
    private const int Path = 1;
    private const int Get = 17;
    private const int Https = 23;
    private const int Ok = 25;
    private const int NotFound = 27;
    private const int PlainTextUtf8 = 54;

    public static byte[] EncodeGet(string authority, string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(authority);
        if (!path.StartsWith('/'))
        {
            throw new ArgumentException("Use an absolute request path.", nameof(path));
        }

        using var block = StartSection();
        WriteIndexed(block, Get);
        WriteIndexed(block, Https);
        WriteNamed(block, Authority, authority);
        if (path == "/")
        {
            WriteIndexed(block, Path);
        }
        else
        {
            WriteNamed(block, Path, path);
        }

        return block.ToArray();
    }

    public static byte[] EncodeResponse(int status)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(status, 200);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(status, 599);

        using var block = StartSection();
        if (status is 200 or 404)
        {
            WriteIndexed(block, status == 200 ? Ok : NotFound);
        }
        else
        {
            WriteNamed(block, Ok, status.ToString(CultureInfo.InvariantCulture));
        }

        WriteIndexed(block, PlainTextUtf8);
        return block.ToArray();
    }

    public static Dictionary<string, string> DecodePseudoHeaders(ReadOnlySpan<byte> block)
    {
        if (!block.StartsWith((ReadOnlySpan<byte>)[0, 0]))
        {
            throw new InvalidDataException("Expected a static QPACK field section.");
        }

        block = block[2..];
        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        bool sawRegularHeader = false;
        while (!block.IsEmpty)
        {
            var (name, value) = ReadField(ref block);
            if (name?.StartsWith(':') == true)
            {
                if (sawRegularHeader || !headers.TryAdd(name, value))
                {
                    throw new InvalidDataException("Pseudo-headers must precede regular fields and appear exactly once.");
                }
            }
            else
            {
                sawRegularHeader = true;
            }
        }

        return headers;
    }

    private static MemoryStream StartSection()
    {
        var block = new MemoryStream();
        block.Write([0, 0]); // Required Insert Count and Delta Base
        return block;
    }

    private static void WriteIndexed(Stream block, int index) => WriteInteger(block, 0xc0, 6, index);

    private static void WriteNamed(Stream block, int nameIndex, string value)
    {
        if (value.Any(c => c is < '!' or > '~'))
        {
            throw new ArgumentException("Use printable ASCII and percent-encode URI components.", nameof(value));
        }

        WriteInteger(block, 0x50, 4, nameIndex);
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        WriteInteger(block, 0, 7, bytes.Length);
        block.Write(bytes);
    }

    private static (string? Name, string Value) ReadField(ref ReadOnlySpan<byte> bytes)
    {
        byte tag = bytes[0];
        if ((tag & 0xc0) == 0xc0) // Indexed field, static table
        {
            return Lookup(ReadInteger(ref bytes, 6));
        }

        if ((tag & 0xd0) == 0x50) // Literal value, static name reference
        {
            var (name, _) = Lookup(ReadInteger(ref bytes, 4));
            return (name, ReadString(ref bytes, 7));
        }

        if ((tag & 0xe0) == 0x20) // Literal name and value
        {
            string name = ReadString(ref bytes, 3);
            return (name, ReadString(ref bytes, 7));
        }

        throw new InvalidDataException("Dynamic QPACK references require a nonzero table capacity.");
    }

    private static (string? Name, string Value) Lookup(int index) => index switch
    {
        Authority => (":authority", ""),
        Path => (":path", "/"),
        >= 15 and <= 21 => (":method", Methods[index - 15]),
        22 => (":scheme", "http"),
        Https => (":scheme", "https"),
        >= 24 and <= 28 => (":status", Statuses[index - 24]),
        >= 63 and <= 71 => (":status", MoreStatuses[index - 63]),
        >= 0 and <= 98 => (null, ""), // Regular headers are outside the application's contract.
        _ => throw new InvalidDataException("Unknown QPACK static table index.")
    };

    private static readonly string[] Methods = ["CONNECT", "DELETE", "GET", "HEAD", "OPTIONS", "POST", "PUT"];
    private static readonly string[] Statuses = ["103", "200", "304", "404", "503"];
    private static readonly string[] MoreStatuses = ["100", "204", "206", "302", "400", "403", "421", "425", "500"];

    private static string ReadString(ref ReadOnlySpan<byte> bytes, int prefixBits)
    {
        if (bytes.IsEmpty)
        {
            throw new InvalidDataException("Missing QPACK string.");
        }

        bool huffman = (bytes[0] & (1 << prefixBits)) != 0;
        int length = ReadInteger(ref bytes, prefixBits);
        if (length > bytes.Length)
        {
            throw new InvalidDataException("Truncated QPACK string.");
        }

        var text = bytes[..length];
        bytes = bytes[length..];
        return huffman ? HpackHuffman.Decode(text) : Encoding.Latin1.GetString(text);
    }

    private static int ReadInteger(ref ReadOnlySpan<byte> bytes, int prefixBits)
    {
        int mask = (1 << prefixBits) - 1;
        long value = bytes[0] & mask;
        bytes = bytes[1..];
        if (value != mask)
        {
            return (int)value;
        }

        for (int shift = 0; shift <= 28 && !bytes.IsEmpty; shift += 7)
        {
            int next = bytes[0];
            bytes = bytes[1..];
            value += (long)(next & 127) << shift;
            if (next < 128 && value <= int.MaxValue)
            {
                return (int)value;
            }
        }

        throw new InvalidDataException("Truncated or oversized QPACK integer.");
    }

    private static void WriteInteger(Stream output, int tag, int prefixBits, int value)
    {
        int mask = (1 << prefixBits) - 1;
        output.WriteByte((byte)(tag | Math.Min(value, mask)));
        if (value < mask)
        {
            return;
        }

        value -= mask;
        while (value >= 128)
        {
            output.WriteByte((byte)((value & 127) | 128));
            value >>= 7;
        }

        output.WriteByte((byte)value);
    }
}
