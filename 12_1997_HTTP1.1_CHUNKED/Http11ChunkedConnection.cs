using System.Text;

internal sealed class Http11ChunkedConnection(Stream stream)
{
    public void Run()
    {
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        while (true)
        {
            var request = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (request is not [var method, var path, ..] || method is not ("GET" or "HEAD"))
            {
                break;
            }

            var versioned = request.Length > 2;
            var http11 = versioned && request[2] == "HTTP/1.1";
            if (!versioned && method == "HEAD")
            {
                break;
            }

            var hosts = new List<string>();
            var close = false;
            var hasBody = false;
            if (versioned)
            {
                string? line;
                while ((line = reader.ReadLine()) is { Length: > 0 })
                {
                    if (line.StartsWith("Host:", StringComparison.OrdinalIgnoreCase))
                    {
                        hosts.Add(line[5..].Trim());
                    }

                    if (line.StartsWith("Connection:", StringComparison.OrdinalIgnoreCase))
                    {
                        close |= line[11..].Split(',').Any(token => token.Trim().Equals("close", StringComparison.OrdinalIgnoreCase));
                    }

                    hasBody |= line.StartsWith("Transfer-Encoding:", StringComparison.OrdinalIgnoreCase)
                        || (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)
                        && line[15..].Trim() != "0");
                }

                if (line is null)
                {
                    break;
                }
            }

            var badHost = http11 && (hosts.Count != 1 || hosts[0].Length == 0 || hosts[0].Contains(','));
            var found = path is "/" or "/chunks";
            var status = found ? "200 OK" : "404 Not Found";
            var html = found ? "<h1>Hello, HTTP!</h1>\n" : "<h1>Not found</h1>\n";
            if (badHost || hasBody)
            {
                status = "400 Bad Request";
                html = "<h1>Bad request</h1>\n";
            }

            var body = Encoding.ASCII.GetBytes(html);
            var keepAlive = http11 && !close && !badHost && !hasBody;
            var chunked = http11 && path == "/chunks" && !badHost && !hasBody;
            var framing = chunked ? "Transfer-Encoding: chunked" : $"Content-Length: {body.Length}";
            if (versioned)
            {
                stream.Write(Encoding.ASCII.GetBytes($"HTTP/1.{(http11 ? 1 : 0)} {status}\r\nContent-Type: text/html\r\n{framing}\r\nConnection: {(keepAlive ? "keep-alive" : "close")}\r\n\r\n"));
            }

            if (method == "GET" && chunked)
            {
                const int firstChunkSize = 4;
                WriteChunk(body.AsSpan(0, firstChunkSize));
                WriteChunk(body.AsSpan(firstChunkSize));
                stream.Write("0\r\n\r\n"u8);
            }
            else if (method == "GET")
            {
                stream.Write(body);
            }

            if (!keepAlive)
            {
                break;
            }
        }
    }

    private void WriteChunk(ReadOnlySpan<byte> bytes)
    {
        stream.Write(Encoding.ASCII.GetBytes($"{bytes.Length:X}\r\n"));
        stream.Write(bytes);
        stream.Write("\r\n"u8);
    }
}
