using System.Net;
using System.Net.Sockets;
using System.Text;

var server = new TcpListener(IPAddress.Loopback, 8080);
server.Start();
Console.WriteLine("HTTP/1.1 on 127.0.0.1:8080. Stop with Ctrl+C.");
while (true)
{
    using var client = server.AcceptTcpClient();
    using var stream = client.GetStream();
    stream.ReadTimeout = stream.WriteTimeout = 5000;
    using var reader = new StreamReader(stream, Encoding.ASCII);
    try
    {
        while (true)
        {
            var request = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (request is not [var method, var path, ..] || method is not ("GET" or "HEAD")) break;
            var versioned = request.Length > 2;
            var http11 = versioned && request[2] == "HTTP/1.1";
            if (!versioned && method == "HEAD") break;
            var hosts = new List<string>();
            var close = false;
            var hasBody = false;
            if (versioned)
            {
                string? line;
                while ((line = reader.ReadLine()) is { Length: > 0 })
                {
                    if (line.StartsWith("Host:", StringComparison.OrdinalIgnoreCase)) hosts.Add(line[5..].Trim());
                    if (line.StartsWith("Connection:", StringComparison.OrdinalIgnoreCase))
                        close |= line[11..].Split(',').Any(token => token.Trim().Equals("close", StringComparison.OrdinalIgnoreCase));
                    hasBody |= line.StartsWith("Transfer-Encoding:", StringComparison.OrdinalIgnoreCase)
                        || (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase) && line[15..].Trim() != "0");
                }
                if (line is null) break;
            }
            var badHost = http11 && (hosts.Count != 1 || hosts[0].Length == 0 || hosts[0].Contains(','));
            var status = path == "/" ? "200 OK" : "404 Not Found";
            var html = path == "/" ? "<h1>Hello, HTTP!</h1>\n" : "<h1>Not found</h1>\n";
            if (badHost || hasBody) { status = "400 Bad Request"; html = "<h1>Bad request</h1>\n"; }
            var body = Encoding.ASCII.GetBytes(html);
            var keepAlive = http11 && !close && !badHost && !hasBody;
            if (versioned)
                stream.Write(Encoding.ASCII.GetBytes($"HTTP/1.{(http11 ? 1 : 0)} {status}\r\nContent-Type: text/html\r\nContent-Length: {body.Length}\r\nConnection: {(keepAlive ? "keep-alive" : "close")}\r\n\r\n"));
            if (method == "GET") stream.Write(body);
            if (!keepAlive) break;
        }
    }
    catch (IOException) { } // A disconnected or timed-out client must not stop the demo.
}
