using System.Text;
using static HeaderCompression;

// Dynamic capacity is advertised as zero: static references and literals suffice.
internal static class Qpack
{
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

    public static List<HeaderField> Decode(byte[] block)
    {
        using var input = new MemoryStream(block);
        if (ReadInteger(input, ReadByte(input), 8) != 0)
        {
            throw new InvalidDataException("QPACK dynamic capacity is zero.");
        }

        var firstBase = ReadByte(input);
        ReadInteger(input, firstBase, 7);
        if ((firstBase & 128) != 0)
        {
            throw new InvalidDataException("QPACK Base underflow.");
        }

        var fields = new List<HeaderField>();
        var fieldSectionSize = 0;
        while (input.Position < input.Length)
        {
            var first = ReadByte(input);
            if ((first & 128) != 0)
            {
                if ((first & 64) == 0)
                {
                    throw new InvalidDataException("Dynamic QPACK reference.");
                }

                fields.Add(At(ReadInteger(input, first, 6)));
            }
            else
            {
                string name;
                if ((first & 64) != 0)
                {
                    if ((first & 16) == 0)
                    {
                        throw new InvalidDataException("Dynamic QPACK name.");
                    }

                    name = At(ReadInteger(input, first, 4)).Name;
                }
                else if ((first & 32) != 0)
                {
                    var length = ReadInteger(input, first, 3);
                    name = ReadText(input, length, (first & 8) != 0, Huffman.Decode);
                }
                else
                {
                    throw new InvalidDataException("QPACK post-base reference with no dynamic table.");
                }

                fields.Add(new(name, ReadText(input, Huffman.Decode)));
            }

            fieldSectionSize += fields[^1].TableSize;
            if (fields.Count > 128 || fieldSectionSize > 65536)
            {
                throw new InvalidDataException("QPACK field section too large.");
            }
        }

        return fields;
    }
}
