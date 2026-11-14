using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

static class QuicServer
{
    public static async Task Run()
    {
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Loopback, 8445));
        var routes = new Dictionary<string, QuicSession>();
        Console.WriteLine("QUIC/TLS 1.3 on 127.0.0.1:8445 UDP. Stop with Ctrl+C.");
        Console.WriteLine($"Certificate SPKI pin: {TlsCertificate.Pin}");
        while (true)
        {
            using var timeout = new CancellationTokenSource(50);
            try
            {
                var datagram = await udp.ReceiveAsync(timeout.Token);
                var bytes = datagram.Buffer;
                while (bytes.Length > 0)
                {
                    QuicPacket packet;
                    try { packet = QuicPacket.Read(bytes); } catch (InvalidDataException) { break; }
                    bytes = bytes[packet.Bytes.Length..];
                    if (packet.Version is not (0 or 1) && datagram.Buffer.Length >= 1200)
                        await udp.SendAsync(QuicPacket.Versions(packet.Source, packet.Destination), datagram.RemoteEndPoint);
                    if (packet.Level < 0) continue;
                    var key = Convert.ToHexString(packet.Destination);
                    if (!routes.TryGetValue(key, out var session))
                    {
                        if (packet.Level != 0 || datagram.Buffer.Length < 1200 || packet.Destination.Length < 8 || routes.Count >= 64) continue;
                        session = new(datagram.RemoteEndPoint, packet.Destination, packet.Source);
                        routes.Add(key, session); routes.Add(Convert.ToHexString(session.Id), session);
                    }
                    if (!session.Peer.Equals(datagram.RemoteEndPoint)) continue;
                    try { session.Receive(packet); }
                    catch (Exception error) when (error is InvalidDataException or CryptographicException)
                    {
                        Console.Error.WriteLine($"QUIC: {error.Message}"); session.Abort();
                    }
                }
            }
            catch (OperationCanceledException) { }
            foreach (var session in routes.Values.Distinct().ToArray())
            {
                List<byte[]> output;
                try { output = session.Flush(); }
                catch (InvalidDataException error) { Console.Error.WriteLine($"HTTP/3: {error.Message}"); session.Abort(true); output = session.Flush(); }
                foreach (var packet in output)
                    try { await udp.SendAsync(packet, session.Peer); } catch (SocketException) { session.Stop(); break; }
                if (!session.Closed && Environment.TickCount64 - session.LastSeen < 10000) continue;
                foreach (var key in routes.Where(pair => pair.Value == session).Select(pair => pair.Key).ToArray()) routes.Remove(key);
                session.Dispose();
            }
        }
    }
}
