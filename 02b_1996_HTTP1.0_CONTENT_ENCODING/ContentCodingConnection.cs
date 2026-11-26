using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

internal static class ContentCodingConnection
{
    private const int BodyLimit = 64 * 1024;
    private static readonly byte[] Page = Encoding.UTF8.GetBytes(
        "<!doctype html><meta charset=utf-8><title>Dev Jungles</title><ul>\n" +
        string.Concat(Enumerable.Range(1, 64).Select(n => $"<li>HTTP · example {n}</li>\n")) +
        "</ul>\n");

    public static void Serve(Stream stream)
    {
        // Read through CRLF CRLF only. Bytes immediately after it may be gzip data.
        var head = new byte[8192];
        for (var i = 0; i < head.Length; i++)
        {
            var next = stream.ReadByte();
            if (next < 0)
            {
                return;
            }

            head[i] = (byte)next;
            if (i >= 3 && head.AsSpan(i - 3, 4).SequenceEqual("\r\n\r\n"u8))
            {
                Dispatch(stream, Encoding.ASCII.GetString(head, 0, i - 3).Split("\r\n"));
                return;
            }
        }

        Reply(stream, "HTTP/1.0", "400 Bad Request");
    }

    private static void Dispatch(Stream stream, string[] lines)
    {
        if (lines[0].Split(' ') is not [var method, var path, var version] ||
            version is not ("HTTP/1.0" or "HTTP/1.1"))
        {
            Reply(stream, "HTTP/1.0", "400 Bad Request");
            return;
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines.Skip(1))
        {
            var colon = line.IndexOf(':');
            if (colon < 1 || !headers.TryAdd(line[..colon], line[(colon + 1)..].Trim()))
            {
                Reply(stream, version, "400 Bad Request");
                return;
            }
        }

        if (method == "GET" && path == "/")
        {
            SendPage(stream, version, headers.GetValueOrDefault("Accept-Encoding", ""));
        }
        else if (method == "POST" && path is "/events" or "/legacy/events" && version == "HTTP/1.1")
        {
            ReceiveEvents(stream, headers, path == "/events");
        }
        else
        {
            Reply(stream, version, "404 Not Found");
        }
    }

    private static void SendPage(Stream stream, string version, string acceptEncoding)
    {
        var preferences = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in acceptEncoding.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!StringWithQualityHeaderValue.TryParse(item, out var preference))
            {
                Reply(stream, version, "400 Bad Request");
                return;
            }

            preferences[preference.Value] = preference.Quality ?? 1;
        }

        var wildcard = preferences.GetValueOrDefault("*");
        var coding = preferences.ContainsKey("x-gzip") && !preferences.ContainsKey("gzip") ? "x-gzip" : "gzip";
        var gzipQuality = preferences.GetValueOrDefault(coding, wildcard);
        var identityAllowed = preferences.GetValueOrDefault("identity",
            preferences.ContainsKey("*") && wildcard == 0 ? 0 : 1) > 0;
        var fields = "Content-Type: text/html; charset=utf-8\r\n" +
            (version == "HTTP/1.1" ? "Vary: Accept-Encoding\r\n" : "");
        if (gzipQuality > 0 && gzipQuality >= preferences.GetValueOrDefault("identity"))
        {
            using var encoded = new MemoryStream();
            using (var gzip = new GZipStream(encoded, CompressionLevel.Optimal, leaveOpen: true))
            {
                gzip.Write(Page);
            }

            Reply(stream, version, "200 OK", fields + $"Content-Encoding: {coding}\r\n", encoded.ToArray());
        }
        else if (identityAllowed)
        {
            Reply(stream, version, "200 OK", fields, Page);
        }
        else
        {
            Reply(stream, version, "406 Not Acceptable");
        }
    }

    // 2015 extension: Accept-Encoding in a response describes what this endpoint can receive.
    private static void ReceiveEvents(Stream stream, Dictionary<string, string> headers, bool supportsGzip)
    {
        const string version = "HTTP/1.1";
        if (headers.ContainsKey("Transfer-Encoding") ||
            !int.TryParse(headers.GetValueOrDefault("Content-Length"), out var length) || length < 0)
        {
            Reply(stream, version, "400 Bad Request");
            return;
        }

        if (length > BodyLimit)
        {
            Reply(stream, version, "413 Content Too Large");
            return;
        }

        var body = new byte[length];
        stream.ReadExactly(body);
        var coding = headers.GetValueOrDefault("Content-Encoding");
        var decodedLength = length;
        if (coding != null)
        {
            if (!supportsGzip || !string.Equals(coding, "gzip", StringComparison.OrdinalIgnoreCase))
            {
                var accepts = supportsGzip ? "gzip" : "identity";
                Reply(stream, version, "415 Unsupported Media Type", $"Accept-Encoding: {accepts}\r\n");
                return;
            }

            try
            {
                using var gzip = new GZipStream(new MemoryStream(body), CompressionMode.Decompress);
                decodedLength = gzip.ReadAtLeast(new byte[BodyLimit + 1], BodyLimit + 1, throwOnEndOfStream: false);
            }
            catch (InvalidDataException)
            {
                Reply(stream, version, "400 Bad Request");
                return;
            }

            if (decodedLength > BodyLimit)
            {
                Reply(stream, version, "413 Content Too Large");
                return;
            }
        }

        Console.WriteLine($"POST: {length} B on wire → {decodedLength} B decoded");
        Reply(stream, version, "204 No Content");
    }

    private static void Reply(Stream stream, string version, string status, string fields = "", byte[]? body = null)
    {
        var length = status == "204 No Content" ? "" : $"Content-Length: {body?.Length ?? 0}\r\n";
        var close = version == "HTTP/1.1" ? "Connection: close\r\n" : "";
        stream.Write(Encoding.ASCII.GetBytes($"{version} {status}\r\n{fields}{length}{close}\r\n"));
        if (body != null)
        {
            stream.Write(body);
        }
    }
}
