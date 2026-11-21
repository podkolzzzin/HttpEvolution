using static Http2Frames;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

internal static class Http2
{
    public static void Run()
    {
        var server = new TcpListener(IPAddress.Loopback, 8081);
        server.Start();
        Console.WriteLine("HTTP/2 on 127.0.0.1:8081. Stop with Ctrl+C.");
        while (true)
        {
            using var client = server.AcceptTcpClient();
            using var stream = client.GetStream();
            stream.ReadTimeout = stream.WriteTimeout = 5000;
            try
            {
                Serve(stream);
            }
            catch (IOException)
            {
            }
            catch (InvalidDataException)
            {
            }
        }
    }

    private static void Serve(Stream stream)
    {
        var preface = new byte[24];
        stream.ReadExactly(preface);
        if (!preface.AsSpan().SequenceEqual("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8))
        {
            return;
        }

        Write(stream, 4, 0, 0, []); // SETTINGS: use the protocol defaults.
        while (true)
        {
            var (type, flags, id, data) = Read(stream);
            if (type == 7)
            {
                return; // GOAWAY
            }

            if (type == 4 && id == 0 && flags == 0 && data.Length % 6 == 0)
            {
                Write(stream, 4, 1, 0, []); // SETTINGS ACK
            }

            if (type == 6 && id == 0 && flags == 0 && data.Length == 8)
            {
                Write(stream, 6, 1, 0, data); // PING ACK echoes the opaque bytes.
            }
        }
    }
}
