using System.Net;
using System.Net.Sockets;
using System.Text;

if (args is [var mode, var pin] && mode is "--ssl3-client" or "--tls10-client")
{
    try { LegacySsl.Client(pin, (byte)(mode == "--ssl3-client" ? 0 : 1)); } catch (IOException error) { Console.Error.WriteLine(error.Message); Environment.ExitCode = 1; }
    return;
}
if (args.Contains("--h2")) { Http2.Run(); return; }

var ssl3 = args.Contains("--ssl3");
var tls10 = args.Contains("--tls10");
var port = ssl3 ? 8442 : tls10 ? 8441 : 8080;
var server = new TcpListener(IPAddress.Loopback, port);
server.Start();
Console.WriteLine($"{(ssl3 ? "HTTPS/SSL 3.0" : tls10 ? "HTTPS/TLS 1.0" : "HTTP/1.1")} on 127.0.0.1:{port}. Stop with Ctrl+C.");
if (ssl3 || tls10) Console.WriteLine($"Certificate SPKI pin: {LegacyIdentity.Pin}");
while (true)
{
    using var client = server.AcceptTcpClient();
    using Stream? stream = ssl3 || tls10 ? LegacySsl.TryAccept(client.GetStream(), (byte)(ssl3 ? 0 : 1)) : client.GetStream();
    if (stream is null) continue;
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
            var found = path is "/" or "/chunks";
            var status = found ? "200 OK" : "404 Not Found";
            var html = found ? "<h1>Hello, HTTP!</h1>\n" : "<h1>Not found</h1>\n";
            if (badHost || hasBody) { status = "400 Bad Request"; html = "<h1>Bad request</h1>\n"; }
            var body = Encoding.ASCII.GetBytes(html);
            var keepAlive = http11 && !close && !badHost && !hasBody;
            var chunked = http11 && path == "/chunks" && !badHost && !hasBody;
            var framing = chunked ? "Transfer-Encoding: chunked" : $"Content-Length: {body.Length}";
            if (versioned)
                stream.Write(Encoding.ASCII.GetBytes($"HTTP/1.{(http11 ? 1 : 0)} {status}\r\nContent-Type: text/html\r\n{framing}\r\nConnection: {(keepAlive ? "keep-alive" : "close")}\r\n\r\n"));
            if (method == "GET" && chunked)
            {
                foreach (var part in new[] { body[..4], body[4..] })
                {
                    stream.Write(Encoding.ASCII.GetBytes($"{part.Length:X}\r\n"));
                    stream.Write(part);
                    stream.Write("\r\n"u8);
                }
                stream.Write("0\r\n\r\n"u8);
            }
            else if (method == "GET") stream.Write(body);
            if (!keepAlive) break;
        }
    }
    catch (IOException) { } // A disconnected or timed-out client must not stop the demo.
}
