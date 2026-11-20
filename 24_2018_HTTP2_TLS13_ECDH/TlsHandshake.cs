using System.Security.Cryptography;

sealed class TlsHandshake(string protocol)
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

    public byte[] AcceptHello(byte[] message)
    {
        if (HandshakeSecret.Length != 0)
        {
            throw new InvalidDataException("Repeated ClientHello.");
        }

        var hello = new TlsHello(message);
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
}
