using System.Text;

sealed class Hpack
{
    // RFC 7541 Appendix A: the 61 fixed entries, in wire-index order.
    static readonly (string Name, string Value)[] Fixed = new[]
    {
        ":authority", ":method=GET", ":method=POST",
        ":path=/", ":path=/index.html", ":scheme=http",
        ":scheme=https", ":status=200", ":status=204",
        ":status=206", ":status=304", ":status=400",
        ":status=404", ":status=500", "accept-charset",
        "accept-encoding=gzip, deflate", "accept-language", "accept-ranges",
        "accept", "access-control-allow-origin", "age",
        "allow", "authorization", "cache-control",
        "content-disposition", "content-encoding", "content-language",
        "content-length", "content-location", "content-range",
        "content-type", "cookie", "date",
        "etag", "expect", "expires",
        "from", "host", "if-match",
        "if-modified-since", "if-none-match", "if-range",
        "if-unmodified-since", "last-modified", "link",
        "location", "max-forwards", "proxy-authenticate",
        "proxy-authorization", "range", "referer",
        "refresh", "retry-after", "server",
        "set-cookie", "strict-transport-security", "transfer-encoding",
        "user-agent", "vary", "via",
        "www-authenticate",
    }.Select(entry => { var parts = entry.Split('=', 2); return (parts[0], parts.Length == 2 ? parts[1] : ""); }).ToArray();
    readonly List<(string Name, string Value)> dynamic = [];
    int capacity = 4096, size;

    public static byte[] Encode(params (string Name, string Value)[] fields)
    {
        using var output = new MemoryStream();
        foreach (var (name, value) in fields) { output.WriteByte(0); PutText(output, name); PutText(output, value); }
        return output.ToArray(); // Literal fields without indexing or Huffman.
    }

    public List<(string Name, string Value)> Decode(byte[] block)
    {
        using var input = new MemoryStream(block);
        var fields = new List<(string Name, string Value)>();
        while (input.Position < input.Length)
        {
            var first = Byte(input);
            if ((first & 128) != 0) fields.Add(At(Integer(input, first, 7)));
            else if ((first & 224) == 32)
            {
                if (fields.Count != 0) throw new InvalidDataException("Late HPACK table update.");
                capacity = Integer(input, first, 5);
                if (capacity > 4096) throw new InvalidDataException("HPACK table exceeds SETTINGS.");
                Trim();
            }
            else
            {
                var indexed = (first & 64) != 0;
                var index = Integer(input, first, indexed ? 6 : 4);
                var name = index == 0 ? Text(input) : At(index).Name;
                var field = (Name: name, Value: Text(input));
                fields.Add(field);
                if (indexed) { dynamic.Insert(0, field); size += Bytes(field); Trim(); }
            }
            if (fields.Count > 128 || fields.Sum(Bytes) > 65536) throw new InvalidDataException("Header list too large.");
        }
        return fields;
    }

    (string Name, string Value) At(int index)
    {
        if (index <= 0 || index > Fixed.Length + dynamic.Count) throw new InvalidDataException("Invalid HPACK index.");
        return index <= Fixed.Length ? Fixed[index - 1] : dynamic[index - Fixed.Length - 1];
    }

    static int Bytes((string Name, string Value) field) => field.Name.Length + field.Value.Length + 32;
    void Trim()
    {
        while (size > capacity) { size -= Bytes(dynamic[^1]); dynamic.RemoveAt(dynamic.Count - 1); }
    }

    public static int Byte(Stream input) => input.ReadByte() is var value && value >= 0 ? value : throw new InvalidDataException("Truncated HPACK.");
    public static void PutInteger(Stream output, int value, int bits, int flags = 0)
    {
        var mask = (1 << bits) - 1;
        output.WriteByte((byte)(flags | Math.Min(value, mask)));
        if (value < mask) return;
        var remaining = value - mask;
        while (remaining >= 128) { output.WriteByte((byte)((remaining & 127) | 128)); remaining >>= 7; }
        output.WriteByte((byte)remaining);
    }
    public static void PutText(Stream output, string value)
    {
        var bytes = Encoding.Latin1.GetBytes(value); PutInteger(output, bytes.Length, 7); output.Write(bytes);
    }
    public static int Integer(Stream input, int first, int bits)
    {
        var mask = (1 << bits) - 1;
        long value = first & mask;
        if (value < mask) return (int)value;
        for (var shift = 0; shift <= 28; shift += 7)
        {
            var next = Byte(input);
            value += (long)(next & 127) << shift;
            if (value > int.MaxValue) break;
            if ((next & 128) == 0) return (int)value;
        }
        throw new InvalidDataException("HPACK integer overflow.");
    }

    public static string Text(Stream input)
    {
        var first = Byte(input);
        var length = Integer(input, first, 7);
        if (length > 65536 || length > input.Length - input.Position) throw new InvalidDataException("Invalid HPACK string length.");
        var bytes = new byte[length];
        input.ReadExactly(bytes);
        if ((first & 128) != 0) bytes = Huffman.Decode(bytes);
        return Encoding.Latin1.GetString(bytes);
    }
}
