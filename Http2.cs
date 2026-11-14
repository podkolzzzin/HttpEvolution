using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

static class Http2
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
            try { Serve(stream); }
            catch (IOException) { }
            catch (InvalidDataException) { }
        }
    }

    static void Serve(Stream stream)
    {
        var preface = new byte[24];
        stream.ReadExactly(preface);
        if (!preface.AsSpan().SequenceEqual("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8)) return;
        Write(stream, 4, 0, 0, []); // SETTINGS: use the protocol defaults.
        while (true)
        {
            var (type, flags, id, data) = Read(stream);
            if (type == 7) return; // GOAWAY
            if (type == 4 && id == 0 && flags == 0 && data.Length % 6 == 0)
                Write(stream, 4, 1, 0, []); // SETTINGS ACK
            if (type == 6 && id == 0 && flags == 0 && data.Length == 8)
                Write(stream, 6, 1, 0, data); // PING ACK echoes the opaque bytes.
        }
    }

    static (byte Type, byte Flags, int Id, byte[] Data) Read(Stream stream)
    {
        Span<byte> header = stackalloc byte[9];
        stream.ReadExactly(header);
        var length = (header[0] << 16) | (header[1] << 8) | header[2];
        if (length > 16384) throw new InvalidDataException("Frame exceeds the advertised maximum.");
        var data = new byte[length];
        stream.ReadExactly(data);
        return (header[3], header[4], BinaryPrimitives.ReadInt32BigEndian(header[5..]) & 0x7fffffff, data);
    }

    static void Write(Stream stream, byte type, byte flags, int id, byte[] data)
    {
        Span<byte> header = stackalloc byte[9];
        header[0] = (byte)(data.Length >> 16); header[1] = (byte)(data.Length >> 8); header[2] = (byte)data.Length;
        header[3] = type; header[4] = flags;
        BinaryPrimitives.WriteInt32BigEndian(header[5..], id);
        stream.Write(header);
        stream.Write(data);
    }
}
