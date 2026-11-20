using System.Text;

// Dynamic capacity is advertised as zero: static references and literals suffice.
static class Qpack
{
    // RFC 9204 Appendix A, zero-based indices.
    static readonly (string Name, string Value)[] Fixed = new[]
    {
        ":authority",
        ":path=/",
        "age=0",
        "content-disposition",
        "content-length=0",
        "cookie",
        "date",
        "etag",
        "if-modified-since",
        "if-none-match",
        "last-modified",
        "link",
        "location",
        "referer",
        "set-cookie",
        ":method=CONNECT",
        ":method=DELETE",
        ":method=GET",
        ":method=HEAD",
        ":method=OPTIONS",
        ":method=POST",
        ":method=PUT",
        ":scheme=http",
        ":scheme=https",
        ":status=103",
        ":status=200",
        ":status=304",
        ":status=404",
        ":status=503",
        "accept=*/*",
        "accept=application/dns-message",
        "accept-encoding=gzip, deflate, br",
        "accept-ranges=bytes",
        "access-control-allow-headers=cache-control",
        "access-control-allow-headers=content-type",
        "access-control-allow-origin=*",
        "cache-control=max-age=0",
        "cache-control=max-age=2592000",
        "cache-control=max-age=604800",
        "cache-control=no-cache",
        "cache-control=no-store",
        "cache-control=public, max-age=31536000",
        "content-encoding=br",
        "content-encoding=gzip",
        "content-type=application/dns-message",
        "content-type=application/javascript",
        "content-type=application/json",
        "content-type=application/x-www-form-urlencoded",
        "content-type=image/gif",
        "content-type=image/jpeg",
        "content-type=image/png",
        "content-type=text/css",
        "content-type=text/html; charset=utf-8",
        "content-type=text/plain",
        "content-type=text/plain;charset=utf-8",
        "range=bytes=0-",
        "strict-transport-security=max-age=31536000",
        "strict-transport-security=max-age=31536000; includesubdomains",
        "strict-transport-security=max-age=31536000; includesubdomains; preload",
        "vary=accept-encoding",
        "vary=origin",
        "x-content-type-options=nosniff",
        "x-xss-protection=1; mode=block",
        ":status=100",
        ":status=204",
        ":status=206",
        ":status=302",
        ":status=400",
        ":status=403",
        ":status=421",
        ":status=425",
        ":status=500",
        "accept-language",
        "access-control-allow-credentials=FALSE",
        "access-control-allow-credentials=TRUE",
        "access-control-allow-headers=*",
        "access-control-allow-methods=get",
        "access-control-allow-methods=get, post, options",
        "access-control-allow-methods=options",
        "access-control-expose-headers=content-length",
        "access-control-request-headers=content-type",
        "access-control-request-method=get",
        "access-control-request-method=post",
        "alt-svc=clear",
        "authorization",
        "content-security-policy=script-src 'none'; object-src 'none'; base-uri 'none'",
        "early-data=1",
        "expect-ct",
        "forwarded",
        "if-range",
        "origin",
        "purpose=prefetch",
        "server",
        "timing-allow-origin=*",
        "upgrade-insecure-requests=1",
        "user-agent",
        "x-forwarded-for",
        "x-frame-options=deny",
        "x-frame-options=sameorigin",
    }.Select(entry =>
    {
        var p = entry.Split('=', 2);
        return (p[0], p.Length == 2 ? p[1] : "");
    }).ToArray();
    static (string Name, string Value) At(int index) =>
        index < Fixed.Length
            ? Fixed[index]
            : throw new InvalidDataException("Invalid QPACK index.");
    public static byte[] Encode(params (string Name, string Value)[] fields)
    {
        using var output = new MemoryStream();
        output.Write(new byte[] { 0, 0 });
        foreach (var (name, value) in fields)
        {
            var bytes = Encoding.Latin1.GetBytes(name);
            Hpack.PutInteger(output, bytes.Length, 3, 0x20);
            output.Write(bytes);
            Hpack.PutText(output, value);
        }

        return output.ToArray();
    }

    public static List<(string Name, string Value)> Decode(byte[] block)
    {
        using var input = new MemoryStream(block);
        if (Hpack.Integer(input, Hpack.Byte(input), 8) != 0)
        {
            throw new InvalidDataException("QPACK dynamic capacity is zero.");
        }

        var firstBase = Hpack.Byte(input);
        Hpack.Integer(input, firstBase, 7);
        if ((firstBase & 128) != 0)
        {
            throw new InvalidDataException("QPACK Base underflow.");
        }

        var fields = new List<(string Name, string Value)>();
        while (input.Position < input.Length)
        {
            var first = Hpack.Byte(input);
            if ((first & 128) != 0)
            {
                if ((first & 64) == 0)
                {
                    throw new InvalidDataException("Dynamic QPACK reference.");
                }

                fields.Add(At(Hpack.Integer(input, first, 6)));
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

                    name = At(Hpack.Integer(input, first, 4)).Name;
                }
                else if ((first & 32) != 0)
                {
                    var length = Hpack.Integer(input, first, 3);
                    if (length > 65536 || length > input.Length - input.Position)
                    {
                        throw new InvalidDataException("Invalid QPACK name length.");
                    }

                    var bytes = new byte[length];
                    input.ReadExactly(bytes);
                    name = Encoding.Latin1.GetString((first & 8) != 0 ? Huffman.Decode(bytes) : bytes);
                }
                else
                {
                    throw new InvalidDataException("QPACK post-base reference with no dynamic table.");
                }

                fields.Add((name, Hpack.Text(input)));
            }

            if (fields.Count > 128 || fields.Sum(field => field.Name.Length + field.Value.Length + 32) > 65536)
            {
                throw new InvalidDataException("QPACK field section too large.");
            }
        }

        return fields;
    }
}
