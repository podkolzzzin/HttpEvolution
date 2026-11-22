using System.Text;
using static HeaderCompression;

internal static class Qpack
{
    public const int MaxFieldSectionSize = 65_536;

    // RFC 9204 Appendix A, zero-based indices.
    private static readonly HeaderField[] Fixed =
    [
        new(":authority"),
        new(":path", "/"),
        new("age", "0"),
        new("content-disposition"),
        new("content-length", "0"),
        new("cookie"),
        new("date"),
        new("etag"),
        new("if-modified-since"),
        new("if-none-match"),
        new("last-modified"),
        new("link"),
        new("location"),
        new("referer"),
        new("set-cookie"),
        new(":method", "CONNECT"),
        new(":method", "DELETE"),
        new(":method", "GET"),
        new(":method", "HEAD"),
        new(":method", "OPTIONS"),
        new(":method", "POST"),
        new(":method", "PUT"),
        new(":scheme", "http"),
        new(":scheme", "https"),
        new(":status", "103"),
        new(":status", "200"),
        new(":status", "304"),
        new(":status", "404"),
        new(":status", "503"),
        new("accept", "*/*"),
        new("accept", "application/dns-message"),
        new("accept-encoding", "gzip, deflate, br"),
        new("accept-ranges", "bytes"),
        new("access-control-allow-headers", "cache-control"),
        new("access-control-allow-headers", "content-type"),
        new("access-control-allow-origin", "*"),
        new("cache-control", "max-age=0"),
        new("cache-control", "max-age=2592000"),
        new("cache-control", "max-age=604800"),
        new("cache-control", "no-cache"),
        new("cache-control", "no-store"),
        new("cache-control", "public, max-age=31536000"),
        new("content-encoding", "br"),
        new("content-encoding", "gzip"),
        new("content-type", "application/dns-message"),
        new("content-type", "application/javascript"),
        new("content-type", "application/json"),
        new("content-type", "application/x-www-form-urlencoded"),
        new("content-type", "image/gif"),
        new("content-type", "image/jpeg"),
        new("content-type", "image/png"),
        new("content-type", "text/css"),
        new("content-type", "text/html; charset=utf-8"),
        new("content-type", "text/plain"),
        new("content-type", "text/plain;charset=utf-8"),
        new("range", "bytes=0-"),
        new("strict-transport-security", "max-age=31536000"),
        new("strict-transport-security", "max-age=31536000; includesubdomains"),
        new("strict-transport-security", "max-age=31536000; includesubdomains; preload"),
        new("vary", "accept-encoding"),
        new("vary", "origin"),
        new("x-content-type-options", "nosniff"),
        new("x-xss-protection", "1; mode=block"),
        new(":status", "100"),
        new(":status", "204"),
        new(":status", "206"),
        new(":status", "302"),
        new(":status", "400"),
        new(":status", "403"),
        new(":status", "421"),
        new(":status", "425"),
        new(":status", "500"),
        new("accept-language"),
        new("access-control-allow-credentials", "FALSE"),
        new("access-control-allow-credentials", "TRUE"),
        new("access-control-allow-headers", "*"),
        new("access-control-allow-methods", "get"),
        new("access-control-allow-methods", "get, post, options"),
        new("access-control-allow-methods", "options"),
        new("access-control-expose-headers", "content-length"),
        new("access-control-request-headers", "content-type"),
        new("access-control-request-method", "get"),
        new("access-control-request-method", "post"),
        new("alt-svc", "clear"),
        new("authorization"),
        new("content-security-policy", "script-src 'none'; object-src 'none'; base-uri 'none'"),
        new("early-data", "1"),
        new("expect-ct"),
        new("forwarded"),
        new("if-range"),
        new("origin"),
        new("purpose", "prefetch"),
        new("server"),
        new("timing-allow-origin", "*"),
        new("upgrade-insecure-requests", "1"),
        new("user-agent"),
        new("x-forwarded-for"),
        new("x-frame-options", "deny"),
        new("x-frame-options", "sameorigin"),
    ];

    private static HeaderField At(int index) =>
        index < Fixed.Length
            ? Fixed[index]
            : throw new InvalidDataException("Invalid QPACK index.");
    public static byte[] Encode(params IReadOnlyList<HeaderField> fields)
    {
        using var output = new MemoryStream();
        output.Write([0, 0]);
        foreach (var (name, value) in fields)
        {
            var bytes = Encoding.Latin1.GetBytes(name);
            WriteInteger(output, bytes.Length, 3, 0x20);
            output.Write(bytes);
            WriteText(output, value);
        }

        return output.ToArray();
    }

    public sealed record FieldSection(byte[] Block, long FieldsOffset, int RequiredInsertCount, long Base);

    public static List<HeaderField> Decode(byte[] block) => Decode(ReadFieldSection(block));

    public static FieldSection ReadFieldSection(byte[] block, QpackDynamicTable? table = null)
    {
        using var input = new MemoryStream(block);
        var encodedCount = ReadInteger(input, ReadByte(input), 8);
        var required = RequiredInsertCount(encodedCount, table);
        var firstBase = ReadByte(input);
        var deltaBase = ReadInteger(input, firstBase, 7);
        var @base = (firstBase & 0x80) != 0 ? required - (long)deltaBase - 1 : required + (long)deltaBase;
        if (@base < 0)
        {
            throw new InvalidDataException("QPACK Base underflow.");
        }

        return new(block, input.Position, required, @base);
    }

    private static int RequiredInsertCount(int encoded, QpackDynamicTable? table)
    {
        if (encoded == 0)
        {
            return 0;
        }

        // RFC 9204 §4.5.1.1. Recover the count from its wrapping wire value.
        if (table is null)
        {
            throw new InvalidDataException("QPACK dynamic capacity is zero.");
        }

        var maxEntries = table.MaxCapacity / 32;
        var fullRange = 2 * maxEntries;
        if (encoded > fullRange)
        {
            throw new InvalidDataException("Invalid QPACK Encoded Insert Count.");
        }

        var maxValue = table.InsertCount + maxEntries;
        var required = (maxValue / fullRange) * fullRange + encoded - 1;
        if (required > maxValue)
        {
            required -= fullRange;
        }

        return required > 0 ? required : throw new InvalidDataException("Invalid QPACK Required Insert Count.");
    }

    public static List<HeaderField> Decode(FieldSection section, QpackDynamicTable? table = null)
    {
        using var input = new MemoryStream(section.Block);
        input.Position = section.FieldsOffset;
        var fields = new List<HeaderField>();
        var fieldSectionSize = 0;
        while (input.Position < input.Length)
        {
            var field = ReadField(input, section, table);
            fields.Add(field);
            fieldSectionSize += field.TableSize;
            if (fields.Count > 128 || fieldSectionSize > MaxFieldSectionSize)
            {
                throw new InvalidDataException("QPACK field section too large.");
            }
        }

        return fields;
    }

    private static HeaderField ReadField(Stream input, FieldSection section, QpackDynamicTable? table)
    {
        var first = ReadByte(input);
        if ((first & 0x80) != 0) // Indexed field line
        {
            var index = ReadInteger(input, first, 6);
            return (first & 0x40) != 0 ? At(index) : DynamicAt(section, table, section.Base - 1 - index);
        }

        if ((first & 0xF0) == 0x10) // Indexed field line with post-Base index
        {
            return DynamicAt(section, table, section.Base + ReadInteger(input, first, 4));
        }

        var name = (first & 0x60) switch
        {
            0x40 or 0x60 => ReadReferencedName(input, first, section, table),
            0x20 => ReadText(input, ReadInteger(input, first, 3), (first & 0x08) != 0, Huffman.Decode),
            _ => DynamicAt(section, table, section.Base + ReadInteger(input, first, 3)).Name
        };
        return new(name, ReadText(input, Huffman.Decode));
    }

    private static string ReadReferencedName(Stream input, int first, FieldSection section, QpackDynamicTable? table)
    {
        var index = ReadInteger(input, first, 4);
        return ((first & 0x10) != 0 ? At(index) : DynamicAt(section, table, section.Base - 1 - index)).Name;
    }

    private static HeaderField DynamicAt(FieldSection section, QpackDynamicTable? table, long index) =>
        table is not null && index < section.RequiredInsertCount
            ? table.At(index)
            : throw new InvalidDataException("QPACK reference exceeds Required Insert Count.");

    public static int ReadEncoderInstructions(byte[] bytes, QpackDynamicTable table)
    {
        using var input = new MemoryStream(bytes);
        while (input.Position < input.Length)
        {
            var start = (int)input.Position;
            try
            {
                ReadEncoderInstruction(input, table);
            }
            catch (InvalidDataException error) when (error.InnerException is EndOfStreamException)
            {
                return start;
            }
        }

        return (int)input.Position;
    }

    private static void ReadEncoderInstruction(Stream input, QpackDynamicTable table)
    {
        var first = ReadByte(input);
        if ((first & 0xE0) == 0x20)
        {
            table.SetCapacity(ReadInteger(input, first, 5));
            return;
        }

        if ((first & 0xE0) == 0)
        {
            table.Insert(table.AtRelative(ReadInteger(input, first, 5)));
            return;
        }

        var name = (first & 0x80) != 0
            ? ReadEncoderNameReference(input, first, table)
            : ReadText(input, ReadInteger(input, first, 5), (first & 0x20) != 0, Huffman.Decode);
        table.Insert(new(name, ReadText(input, Huffman.Decode)));
    }

    private static string ReadEncoderNameReference(Stream input, int first, QpackDynamicTable table)
    {
        var index = ReadInteger(input, first, 6);
        return ((first & 0x40) != 0 ? At(index) : table.AtRelative(index)).Name;
    }
}
