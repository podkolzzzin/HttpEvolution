using static HeaderCompression;

internal sealed class Hpack(HuffmanDecoder? decodeHuffman = null)
{
    private const int DefaultCapacity = 4096;

    // RFC 7541 Appendix A: the 61 fixed entries, in wire-index order.
    private static readonly HeaderField[] Fixed =
    [
        new(":authority"),
        new(":method", "GET"),
        new(":method", "POST"),
        new(":path", "/"),
        new(":path", "/index.html"),
        new(":scheme", "http"),
        new(":scheme", "https"),
        new(":status", "200"),
        new(":status", "204"),
        new(":status", "206"),
        new(":status", "304"),
        new(":status", "400"),
        new(":status", "404"),
        new(":status", "500"),
        new("accept-charset"),
        new("accept-encoding", "gzip, deflate"),
        new("accept-language"),
        new("accept-ranges"),
        new("accept"),
        new("access-control-allow-origin"),
        new("age"),
        new("allow"),
        new("authorization"),
        new("cache-control"),
        new("content-disposition"),
        new("content-encoding"),
        new("content-language"),
        new("content-length"),
        new("content-location"),
        new("content-range"),
        new("content-type"),
        new("cookie"),
        new("date"),
        new("etag"),
        new("expect"),
        new("expires"),
        new("from"),
        new("host"),
        new("if-match"),
        new("if-modified-since"),
        new("if-none-match"),
        new("if-range"),
        new("if-unmodified-since"),
        new("last-modified"),
        new("link"),
        new("location"),
        new("max-forwards"),
        new("proxy-authenticate"),
        new("proxy-authorization"),
        new("range"),
        new("referer"),
        new("refresh"),
        new("retry-after"),
        new("server"),
        new("set-cookie"),
        new("strict-transport-security"),
        new("transfer-encoding"),
        new("user-agent"),
        new("vary"),
        new("via"),
        new("www-authenticate"),
    ];

    private readonly List<HeaderField> dynamic = [];
    private int capacity = DefaultCapacity;
    private int size;

    public List<HeaderField> Decode(byte[] block)
    {
        using var input = new MemoryStream(block);
        var fields = new List<HeaderField>();
        var headerListSize = 0;
        while (input.Position < input.Length)
        {
            var first = ReadByte(input);
            if ((first & 128) != 0)
            {
                var field = At(ReadInteger(input, first, 7));
                fields.Add(field);
                headerListSize += field.TableSize;
            }
            else if ((first & 224) == 32)
            {
                if (fields.Count != 0)
                {
                    throw new InvalidDataException("Late HPACK table update.");
                }

                capacity = ReadInteger(input, first, 5);
                if (capacity > DefaultCapacity)
                {
                    throw new InvalidDataException("HPACK table exceeds SETTINGS.");
                }

                Trim();
            }
            else
            {
                var indexed = (first & 64) != 0;
                var index = ReadInteger(input, first, indexed ? 6 : 4);
                var name = index == 0 ? ReadText(input, decodeHuffman) : At(index).Name;
                var field = new HeaderField(name, ReadText(input, decodeHuffman));
                fields.Add(field);
                headerListSize += field.TableSize;
                if (indexed)
                {
                    dynamic.Insert(0, field);
                    size += field.TableSize;
                    Trim();
                }
            }

            if (fields.Count > 128 || headerListSize > 65536)
            {
                throw new InvalidDataException("Header list too large.");
            }
        }

        return fields;
    }

    private HeaderField At(int index)
    {
        if (index <= 0 || index > Fixed.Length + dynamic.Count)
        {
            throw new InvalidDataException("Invalid HPACK index.");
        }

        return index <= Fixed.Length ? Fixed[index - 1] : dynamic[index - Fixed.Length - 1];
    }

    private void Trim()
    {
        while (size > capacity)
        {
            size -= dynamic[^1].TableSize;
            dynamic.RemoveAt(dynamic.Count - 1);
        }
    }
}
