// 09_1996_HTTP1.0_SSL3_CLIENT: SSL3: TcpClient із перевіркою SPKI pin.
using System.Net;
using System.Net.Sockets;
using System.Text;

if (args.Length == 0) args = ["--ssl3"];

if (args is [var mode, var pin] && mode is "--ssl3-client")
{
    try { LegacySsl.Client(pin); } catch (IOException error) { Console.Error.WriteLine(error.Message); Environment.ExitCode = 1; }
    return;
}

var ssl3 = args.Contains("--ssl3");
var port = ssl3 ? 8442 : 8080;
var server = new TcpListener(IPAddress.Loopback, port);
server.Start();
Console.WriteLine($"{(ssl3 ? "HTTPS/SSL 3.0" : "HTTP/1.0")} on 127.0.0.1:{port}. Stop with Ctrl+C.");
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
        if (!versioned && method == "HEAD") continue;
        if (versioned)
            while (reader.ReadLine() is { Length: > 0 }) { }
        var status = path == "/" ? "200 OK" : "404 Not Found";
        var html = path == "/" ? "<h1>Hello, HTTP!</h1>\n" : "<h1>Not found</h1>\n";
        var body = Encoding.ASCII.GetBytes(html);
        if (versioned)
            stream.Write(Encoding.ASCII.GetBytes($"HTTP/1.0 {status}\r\nContent-Type: text/html\r\nContent-Length: {body.Length}\r\n\r\n"));
        if (method == "GET") stream.Write(body);
    }
    catch (IOException) { } // A disconnected or timed-out client must not stop the demo.
}
