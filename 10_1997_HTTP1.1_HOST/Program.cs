// 10_1997_HTTP1.1_HOST: Обов’язковий Host для HTTP/1.1.
using System.Net;
using System.Net.Sockets;
using System.Text;

if (args is [var mode, var pin] && mode is "--ssl3-client")
{
    try { LegacySsl.Client(pin); } catch (IOException error) { Console.Error.WriteLine(error.Message); Environment.ExitCode = 1; }
    return;
}

var ssl3 = args.Contains("--ssl3");
var port = ssl3 ? 8442 : 8080;
var server = new TcpListener(IPAddress.Loopback, port);
server.Start();
Console.WriteLine($"{(ssl3 ? "HTTPS/SSL 3.0" : "HTTP/1.1")} on 127.0.0.1:{port}. Stop with Ctrl+C.");
if (ssl3) Console.WriteLine($"Certificate SPKI pin: {LegacyIdentity.Pin}");
while (true)
{
    using var client = server.AcceptTcpClient();
    using Stream? stream = ssl3 ? LegacySsl.TryAccept(client.GetStream()) : client.GetStream();
    if (stream is null) continue;
    stream.ReadTimeout = stream.WriteTimeout = 5000;
    using var reader = new StreamReader(stream, Encoding.ASCII);
    try
    {
        var request = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (request is not [var method, var path, ..] || method is not ("GET" or "HEAD")) continue;
        var versioned = request.Length > 2;
        var http11 = versioned && request[2] == "HTTP/1.1";
        if (!versioned && method == "HEAD") continue;
        var hosts = new List<string>();
        if (versioned)
            while (reader.ReadLine() is { Length: > 0 } line)
                if (line.StartsWith("Host:", StringComparison.OrdinalIgnoreCase)) hosts.Add(line[5..].Trim());
        var badHost = http11 && (hosts.Count != 1 || hosts[0].Length == 0 || hosts[0].Contains(','));
        var status = path == "/" ? "200 OK" : "404 Not Found";
        var html = path == "/" ? "<h1>Hello, HTTP!</h1>\n" : "<h1>Not found</h1>\n";
        if (badHost) { status = "400 Bad Request"; html = "<h1>Bad Host</h1>\n"; }
        var body = Encoding.ASCII.GetBytes(html);
        if (versioned)
            stream.Write(Encoding.ASCII.GetBytes($"HTTP/1.{(http11 ? 1 : 0)} {status}\r\nContent-Type: text/html\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n"));
        if (method == "GET") stream.Write(body);
    }
    catch (IOException) { } // A disconnected or timed-out client must not stop the demo.
}
