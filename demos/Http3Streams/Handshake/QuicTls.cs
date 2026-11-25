using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Http3Streams.Handshake;

// TLS handshake messages travel in QUIC CRYPTO frames, without a TLS record layer.
internal abstract class QuicTls(byte[] localParameters) : IDisposable
{
    private readonly IncrementalHash transcript = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    private readonly ECDiffieHellman ephemeral = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
    private byte[] handshakeSecret = [];
    protected byte[] LocalParameters { get; } = localParameters;
    public byte[]? PeerParameters
    {
        get; protected set;
    }
    public byte[] ClientHandshakeTraffic { get; private set; } = [];
    public byte[] ServerHandshakeTraffic { get; private set; } = [];
    public byte[] ClientTraffic { get; private set; } = [];
    public byte[] ServerTraffic { get; private set; } = [];
    public bool Complete
    {
        get; protected set;
    }

    public static QuicTls Client(string expectedFingerprint, byte[] localParameters) =>
        new ClientHandshake(expectedFingerprint, localParameters);

    public static QuicTls Server(X509Certificate2 certificate, byte[] localParameters) =>
        new ServerHandshake(certificate, localParameters);

    public virtual byte[] StartClientHello() => throw new InvalidOperationException("Only a client sends ClientHello.");

    public IReadOnlyList<(int Level, byte[] Bytes)> Receive(int level, byte[] message)
    {
        var packet = new TlsBytes(message);
        int type = packet.ReadInt(1);
        var body = new TlsBytes(packet.ReadVector(3));
        packet.End();
        return Handle(level, type, body, message);
    }

    protected abstract IReadOnlyList<(int Level, byte[] Bytes)> Handle(int level, int type, TlsBytes body, byte[] message);

    protected byte[] Record(byte[] message)
    {
        transcript.AppendData(message);
        return message;
    }

    protected void ReplaceFirstHelloWithHash()
    {
        Record(TlsBytes.Message(254, transcript.GetHashAndReset()));
    }

    protected byte[] PublicShare()
    {
        var point = ephemeral.ExportParameters(false).Q;
        return [4, .. point.X!, .. point.Y!];
    }

    protected void DeriveHandshakeKeys(byte[] peerShare)
    {
        if (peerShare.Length != 65 || peerShare[0] != 4)
        {
            throw new InvalidDataException("Expected an uncompressed P-256 point.");
        }

        using var peer = ECDiffieHellman.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint { X = peerShare[1..33], Y = peerShare[33..] }
        });
        using var peerPublic = peer.PublicKey;
        byte[] shared = ephemeral.DeriveRawSecretAgreement(peerPublic);
        byte[] early = HMACSHA256.HashData(new byte[32], new byte[32]);
        handshakeSecret = DeriveNextSecret(early, shared);
        CryptographicOperations.ZeroMemory(shared);
        var hash = transcript.GetCurrentHash();
        ClientHandshakeTraffic = TlsHkdf.ExpandLabel(handshakeSecret, "c hs traffic", hash);
        ServerHandshakeTraffic = TlsHkdf.ExpandLabel(handshakeSecret, "s hs traffic", hash);
    }

    protected void DeriveApplicationKeys()
    {
        byte[] master = DeriveNextSecret(handshakeSecret, new byte[32]);
        var hash = transcript.GetCurrentHash();
        ClientTraffic = TlsHkdf.ExpandLabel(master, "c ap traffic", hash);
        ServerTraffic = TlsHkdf.ExpandLabel(master, "s ap traffic", hash);
        CryptographicOperations.ZeroMemory(master);
    }

    private static byte[] DeriveNextSecret(byte[] previous, byte[] input)
    {
        var salt = TlsHkdf.ExpandLabel(previous, "derived", SHA256.HashData(Array.Empty<byte>()));
        var next = HMACSHA256.HashData(salt, input);
        CryptographicOperations.ZeroMemory(salt);
        return next;
    }

    protected byte[] Finished(byte[] secret)
    {
        byte[] key = TlsHkdf.ExpandLabel(secret, "finished", []);
        var tag = HMACSHA256.HashData(key, transcript.GetCurrentHash());
        CryptographicOperations.ZeroMemory(key);
        return tag;
    }

    protected void VerifyFinished(TlsBytes body, byte[] secret)
    {
        if (!CryptographicOperations.FixedTimeEquals(body.Read(body.Remaining), Finished(secret)))
        {
            throw new CryptographicException("TLS Finished verification failed.");
        }
    }

    protected byte[] CertificateVerifyInput()
    {
        byte[] context = Encoding.ASCII.GetBytes(new string(' ', 64) + "TLS 1.3, server CertificateVerify\0");
        return [.. context, .. transcript.GetCurrentHash()];
    }

    public virtual void Dispose()
    {
        ephemeral.Dispose();
        transcript.Dispose();
        foreach (var key in new[] { handshakeSecret, ClientHandshakeTraffic, ServerHandshakeTraffic, ClientTraffic, ServerTraffic })
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }
}
