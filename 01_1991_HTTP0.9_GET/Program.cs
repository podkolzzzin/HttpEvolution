using System.Net;
using System.Net.Sockets;
using System.Text;

var server = new TcpListener(IPAddress.Loopback, 8080);
server.Start();
Console.WriteLine("HTTP/0.9 on 127.0.0.1:8080. Stop with Ctrl+C.");
while (true)
{
    using var client = server.AcceptTcpClient();
    using var stream = client.GetStream();
    stream.ReadTimeout = stream.WriteTimeout = 5000;
    using var reader = new StreamReader(stream, Encoding.ASCII);
    try
    {
        var request = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (request is not ["GET", var path, ..])
        {
            continue;
        }

        var html = path == "/" ? "<h1>Hello, HTTP/0.9!</h1>\n" : "<h1>Not found</h1>\n";
        stream.Write(Encoding.ASCII.GetBytes(html));
    }
    catch (IOException)
    {
    } // A disconnected or timed-out client must not stop the demo.
}
