using System.Net;
using System.Net.Sockets;

internal static class Http2Server
{
    public const int CleartextPort = 8081;

    public static void Run(Func<Stream, Http2Connection> connect) =>
        Run(connect, CleartextPort, "HTTP/2", stream => stream);

    public static void Run(
        Func<Stream, Http2Connection> connect,
        int port,
        string protocol,
        Func<Stream, Stream?> accept)
    {
        var server = new TcpListener(IPAddress.Loopback, port);
        server.Start();
        Console.WriteLine($"{protocol} on 127.0.0.1:{port}. Stop with Ctrl+C.");

        while (true)
        {
            using var client = server.AcceptTcpClient();
            using var stream = accept(client.GetStream());
            if (stream is null)
            {
                continue;
            }

            stream.ReadTimeout = stream.WriteTimeout = 5000;
            try
            {
                connect(stream).Run();
            }
            catch (IOException)
            {
            }
            catch (InvalidDataException)
            {
            }
        }
    }
}
