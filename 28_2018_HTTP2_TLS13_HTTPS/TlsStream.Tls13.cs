using System.Security.Cryptography;

sealed partial class TlsStream
{
    public static TlsStream? TryAccept(Stream transport, string protocol)
    {
        var records = new TlsRecords(transport);
        try
        {
            if (transport.CanTimeout) transport.ReadTimeout = transport.WriteTimeout = 5000;
            using var handshake = new TlsHandshake(protocol);
            do { records.Write(22, handshake.AcceptHello(records.Handshake())); }
            while (handshake.HandshakeSecret.Length == 0);
            records.Keys(handshake.ClientHandshakeTraffic, handshake.ServerHandshakeTraffic);
            foreach (var message in handshake.ServerFlight()) records.Write(22, message);
            handshake.VerifyFinished(records.Handshake());
            records.Compatibility = false;
            records.Keys(handshake.ClientTraffic, handshake.ServerTraffic);
            return new TlsStream(records);
        }
        catch (Exception error) when (error is IOException or InvalidDataException or CryptographicException)
        {
            try { records.Write(21, [2, 40]); } catch (IOException) { }
            records.Dispose(); return null;
        }
    }
}
