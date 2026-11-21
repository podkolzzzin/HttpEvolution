using System.Net;
using System.Net.Sockets;

internal static class Http1Server
{
    public const int CleartextPort = 8080;
    public const int Ssl3Port = 8442;
    public const int Tls10Port = 8441;
    public const int Tls13Port = 8443;

    public static void Run(Action<Stream> serve, string protocol) =>
        Run(serve, protocol, CleartextPort, stream => stream);

    public static void Run(Action<Stream> serve, string protocol, int port, Func<Stream, Stream?> accept, string? pin = null)
    {
        var server = new TcpListener(IPAddress.Loopback, port);
        server.Start();
        Console.WriteLine($"{protocol} on 127.0.0.1:{port}. Stop with Ctrl+C.");
        if (pin is not null)
        {
            Console.WriteLine($"Certificate SPKI pin: {pin}");
        }

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
                serve(stream);
            }
            catch (IOException)
            {
                // A disconnected or timed-out client must not stop the demo.
            }
        }
    }
}
