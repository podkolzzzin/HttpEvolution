using System.Security.Cryptography;
using System.Text;

sealed class TlsHandshake(string protocol) : IDisposable
{
    TlsHello? retry;
    public byte[] Transcript
    {
        get;
        private set;
    } = [];
    public byte[] HandshakeSecret
    {
        get;
        private set;
    } = [];
    public byte[] ClientHandshakeTraffic
    {
        get;
        private set;
    } = [];
    public byte[] ServerHandshakeTraffic
    {
        get;
        private set;
    } = [];
    public string? Protocol
    {
        get;
        private set;
    }
    public Dictionary<int, byte[]> PeerExtensions
    {
        get;
        private set;
    } = [];
    public byte[] ClientTraffic
    {
        get;
        private set;
    } = [];
    public byte[] ServerTraffic
    {
        get;
        private set;
    } = [];
    public bool Complete
    {
        get;
        private set;
    }

    public byte[] AcceptHello(byte[] message)
    {
        if (HandshakeSecret.Length != 0)
        {
            throw new InvalidDataException("Repeated ClientHello.");
        }

        var hello = new TlsHello(message);
        PeerExtensions = hello.Extensions;
        if (hello.Protocols.Contains(protocol))
        {
            Protocol = protocol;
        }
        else if (protocol != "http/1.1" || hello.Protocols.Count != 0)
        {
            throw new InvalidDataException("No shared ALPN protocol.");
        }

        if (hello.Share.Length == 0)
        {
            if (retry is not null)
            {
                throw new InvalidDataException("Repeated HelloRetryRequest.");
            }

            retry = hello;
            var reply = hello.Reply(null);
            Transcript = [.. Wire.Message(254, SHA256.HashData(message)), .. reply];
            return reply;
        }

        if (retry is not null
            && (!hello.Random.AsSpan().SequenceEqual(retry.Random)
            || !hello.Session.AsSpan().SequenceEqual(retry.Session)))
        {
            throw new InvalidDataException("ClientHello changed after retry.");
        }

        if (hello.Share.Length != 65 || hello.Share[0] != 4)
        {
            throw new InvalidDataException("Invalid P-256 point.");
        }

        using var key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        using var peer = ECDiffieHellman.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, Q = new ECPoint { X = hello.Share[1..33], Y = hello.Share[33..65] } });
        var point = key.ExportParameters(false).Q;
        var serverHello = hello.Reply([4, .. point.X!, .. point.Y!]);
        Transcript = [.. Transcript, .. message, .. serverHello];
        using var publicKey = peer.PublicKey;
        var shared = key.DeriveRawSecretAgreement(publicKey);
        var early = TlsCrypto.Extract(new byte[32], new byte[32]);
        HandshakeSecret = TlsCrypto.Extract(TlsCrypto.Derive(early, "derived", []), shared);
        CryptographicOperations.ZeroMemory(shared);
        ClientHandshakeTraffic = TlsCrypto.Derive(HandshakeSecret, "c hs traffic", Transcript);
        ServerHandshakeTraffic = TlsCrypto.Derive(HandshakeSecret, "s hs traffic", Transcript);
        return serverHello;
    }

    public byte[][] ServerFlight(byte[]? transportParameters = null)
    {
        if (HandshakeSecret.Length == 0 || ClientTraffic.Length != 0)
        {
            throw new InvalidDataException("Invalid handshake stage.");
        }

        var flight = new List<byte[]>();
        void Add(byte type, byte[] body)
        {
            var message = Wire.Message(type, body);
            flight.Add(message);
            Transcript = [.. Transcript, .. message];
        }

        byte[] extensions = Protocol is null
            ? []
            : Wire.Extension(
                16,
                Wire.Vector(Wire.Vector(Encoding.ASCII.GetBytes(Protocol), 1))
            );
        if (transportParameters is not null)
        {
            if (!PeerExtensions.ContainsKey(57))
            {
                throw new InvalidDataException("Missing QUIC transport parameters.");
            }

            extensions = [.. extensions, .. Wire.Extension(57, transportParameters)];
        }

        Add(8, Wire.Vector(extensions)); // EncryptedExtensions
        Add(11, [0, .. Wire.Vector([.. Wire.Vector(TlsCertificate.Der, 3), 0, 0], 3)]);
        Add(15, [4, 3, .. Wire.Vector(TlsCertificate.Sign(Transcript))]); // ECDSA CertificateVerify
        Add(20, TlsCrypto.Finished(ServerHandshakeTraffic, Transcript));
        var master = TlsCrypto.Extract(TlsCrypto.Derive(HandshakeSecret, "derived", []), new byte[32]);
        ClientTraffic = TlsCrypto.Derive(master, "c ap traffic", Transcript);
        ServerTraffic = TlsCrypto.Derive(master, "s ap traffic", Transcript);
        CryptographicOperations.ZeroMemory(master);
        return flight.ToArray();
    }

    public void VerifyFinished(byte[] message)
    {
        if (ClientTraffic.Length == 0 || Complete
            || message.Length != 36
            || !message.AsSpan(0, 4).SequenceEqual(new byte[] { 20, 0, 0, 32 }))
        {
            throw new InvalidDataException("Expected client Finished.");
        }

        if (!CryptographicOperations.FixedTimeEquals(
            TlsCrypto.Finished(ClientHandshakeTraffic, Transcript),
            message.AsSpan(4)
        ))
        {
            throw new InvalidDataException("Client Finished verification failed.");
        }

        Transcript = [.. Transcript, .. message];
        Complete = true;
    }

    public void Dispose()
    {
        foreach (var secret in new[]
        {
            HandshakeSecret,
            ClientHandshakeTraffic,
            ServerHandshakeTraffic,
            ClientTraffic,
            ServerTraffic
        }

        )
        {
            CryptographicOperations.ZeroMemory(secret);
        }
    }
}
