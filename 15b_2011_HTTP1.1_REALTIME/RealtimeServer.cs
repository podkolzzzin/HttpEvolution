using System.Text;

internal static class RealtimeServer
{
    public static void Run(bool webSocket)
    {
        var protocol = webSocket ? "WebSocket echo" : "Server-Sent Events";
        Http1Server.Run(stream => Serve(stream, webSocket), protocol, 8082, stream => stream);
    }

    private static void Serve(Stream stream, bool webSocket)
    {
        var request = ReadRequest(stream);
        if (request is null)
        {
            stream.Write("HTTP/1.1 400 Bad Request\r\nConnection: close\r\nContent-Length: 0\r\n\r\n"u8);
            return;
        }

        var (path, headers) = request.Value;
        if (path == "/")
        {
            var html = Encoding.UTF8.GetBytes(webSocket ? WebSocketEcho.Page : ServerSentEvents.Page);
            stream.Write(Encoding.ASCII.GetBytes(
                $"HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {html.Length}\r\nConnection: close\r\n\r\n"));
            stream.Write(html);
        }
        else if (webSocket && path == "/chat")
        {
            WebSocketEcho.Run(stream, headers);
        }
        else if (!webSocket && path == "/events")
        {
            ServerSentEvents.Run(stream, headers.GetValueOrDefault("Last-Event-ID"));
        }
        else
        {
            stream.Write("HTTP/1.1 404 Not Found\r\nConnection: close\r\nContent-Length: 0\r\n\r\n"u8);
        }
    }

    private static (string Path, Dictionary<string, string> Headers)? ReadRequest(Stream stream)
    {
        // Stop at CRLF CRLF. A buffered text reader could consume the first WebSocket frame.
        var bytes = new byte[8192];
        for (var i = 0; i < bytes.Length; i++)
        {
            var value = stream.ReadByte();
            if (value < 0)
            {
                return null;
            }

            bytes[i] = (byte)value;
            if (i < 3 || !bytes.AsSpan(i - 3, 4).SequenceEqual("\r\n\r\n"u8))
            {
                continue;
            }

            var lines = Encoding.ASCII.GetString(bytes, 0, i - 3).Split("\r\n");
            if (lines[0].Split(' ') is not ["GET", var path, "HTTP/1.1"])
            {
                return null;
            }

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines.Skip(1))
            {
                var colon = line.IndexOf(':');
                if (colon <= 0 || !headers.TryAdd(line[..colon], line[(colon + 1)..].Trim()))
                {
                    return null;
                }
            }

            if (string.IsNullOrWhiteSpace(headers.GetValueOrDefault("Host")) ||
                headers.ContainsKey("Transfer-Encoding") || headers.GetValueOrDefault("Content-Length", "0") != "0")
            {
                return null;
            }

            return (path, headers);
        }

        return null;
    }
}
