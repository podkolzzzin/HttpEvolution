using System.Text;

internal sealed class Http11HostConnection(Stream stream)
{
    public void Run()
    {
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var request = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (request is not [var method, var path, ..] || method is not ("GET" or "HEAD"))
        {
            return;
        }

        var versioned = request.Length > 2;
        var http11 = versioned && request[2] == "HTTP/1.1";
        if (!versioned && method == "HEAD")
        {
            return;
        }

        var hosts = new List<string>();
        if (versioned)
        {
            while (reader.ReadLine() is { Length: > 0 } line)
            {
                if (line.StartsWith("Host:", StringComparison.OrdinalIgnoreCase))
                {
                    hosts.Add(line[5..].Trim());
                }
            }
        }

        var badHost = http11 && (hosts.Count != 1 || hosts[0].Length == 0 || hosts[0].Contains(','));
        var status = path == "/" ? "200 OK" : "404 Not Found";
        var html = path == "/" ? "<h1>Hello, HTTP!</h1>\n" : "<h1>Not found</h1>\n";
        if (badHost)
        {
            status = "400 Bad Request";
            html = "<h1>Bad Host</h1>\n";
        }

        var body = Encoding.ASCII.GetBytes(html);
        if (versioned)
        {
            stream.Write(Encoding.ASCII.GetBytes($"HTTP/1.{(http11 ? 1 : 0)} {status}\r\nContent-Type: text/html\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n"));
        }

        if (method == "GET")
        {
            stream.Write(body);
        }
    }
}
