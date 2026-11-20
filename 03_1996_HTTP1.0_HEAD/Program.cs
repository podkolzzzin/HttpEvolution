// 03_1996_HTTP1.0_HEAD: HEAD: заголовки без тіла відповіді.
using System.Net;
using System.Net.Sockets;
using System.Text;

var server = new TcpListener(IPAddress.Loopback, 8080);
server.Start();
Console.WriteLine("HTTP/1.0 on 127.0.0.1:8080. Stop with Ctrl+C.");
while (true)
{
    using var client = server.AcceptTcpClient();
    using var stream = client.GetStream();
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
