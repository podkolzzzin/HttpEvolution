using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Http3Streams.Handshake;

internal sealed class ServerHandshake(X509Certificate2 certificate, byte[] parameters) : QuicTls(parameters)
{
    private static readonly byte[] RetryRandom =
        Convert.FromHexString("CF21AD74E59A6111BE1D8C021E65B891C2A211167ABB8C5E079E09E2C8A8339C");

    private byte[]? firstRandom;
    private byte[]? firstSession;

    protected override IReadOnlyList<(int Level, byte[] Bytes)> Handle(int level, int type, TlsBytes body, byte[] message)
    {
        if (level == 0 && type == 1 && ServerHandshakeTraffic.Length == 0)
        {
            return AcceptHello(body, message);
        }

        if (level == 1 && type == 20 && ServerTraffic.Length != 0 && !Complete)
        {
            VerifyFinished(body, ClientHandshakeTraffic);
            Record(message);
            Complete = true;
            return [];
        }

        throw new InvalidDataException("Unexpected TLS message from client.");
    }

    private IReadOnlyList<(int Level, byte[] Bytes)> AcceptHello(TlsBytes body, byte[] message)
    {
        if (body.ReadInt() != 0x0303)
        {
            throw new InvalidDataException("Invalid ClientHello legacy version.");
        }

        byte[] random = body.Read(32);
        byte[] session = body.ReadVector(1);
        if (session.Length > 32 || !ContainsPair(body.ReadVector(), 0x1301)
            || !body.ReadVector(1).AsSpan().SequenceEqual((byte[])[0]))
        {
            throw new InvalidDataException("Expected AES-128-GCM and null compression.");
        }

        var extensions = body.ReadExtensions();
        body.End();
        if (!ContainsPair(TlsBytes.Unwrap(TlsBytes.Required(extensions, 43), 1), 0x0304)
            || !ContainsPair(TlsBytes.Unwrap(TlsBytes.Required(extensions, 13)), 0x0403)
            || !ContainsPair(TlsBytes.Unwrap(TlsBytes.Required(extensions, 10)), 23))
        {
            throw new InvalidDataException("TLS 1.3 with P-256 and ECDSA is required.");
        }

        var alpn = new TlsBytes(TlsBytes.Unwrap(TlsBytes.Required(extensions, 16)));
        bool offeredHttp3 = false;
        while (alpn.Remaining > 0)
        {
            offeredHttp3 |= alpn.ReadVector(1).AsSpan().SequenceEqual("h3"u8);
        }
        if (!offeredHttp3)
        {
            throw new InvalidDataException("Client did not offer h3.");
        }

        PeerParameters = TlsBytes.Required(extensions, 57);
        var shares = new TlsBytes(TlsBytes.Unwrap(TlsBytes.Required(extensions, 51)));
        byte[]? peerShare = null;
        while (shares.Remaining > 0)
        {
            int group = shares.ReadInt();
            byte[] key = shares.ReadVector();
            if (group == 23)
            {
                if (peerShare is not null)
                {
                    throw new InvalidDataException("Duplicate P-256 key share.");
                }
                peerShare = key;
            }
        }

        Record(message);
        if (peerShare is null)
        {
            if (firstRandom is not null)
            {
                throw new InvalidDataException("Missing P-256 share after retry.");
            }
            firstRandom = random;
            firstSession = session;
            ReplaceFirstHelloWithHash();
            return [(0, Record(ServerHello(session, RetryRandom, [0, 23])))];
        }

        if (firstRandom is not null && (!random.AsSpan().SequenceEqual(firstRandom)
            || !session.AsSpan().SequenceEqual(firstSession)))
        {
            throw new InvalidDataException("Client changed identity across HelloRetryRequest.");
        }

        byte[] hello = Record(ServerHello(session, RandomNumberGenerator.GetBytes(32), [0, 23, .. TlsBytes.Vector(PublicShare())]));
        DeriveHandshakeKeys(peerShare);
        return [(0, hello), (1, CreateServerFlight())];
    }

    private static bool ContainsPair(byte[] data, int expected)
    {
        var vector = new TlsBytes(data);
        bool found = false;
        while (vector.Remaining > 0)
        {
            found |= vector.ReadInt() == expected;
        }
        return found;
    }

    private static byte[] ServerHello(byte[] session, byte[] random, byte[] keyShare)
    {
        byte[] fields = [.. TlsBytes.Extension(43, [3, 4]), .. TlsBytes.Extension(51, keyShare)];
        return TlsBytes.Message(2, [3, 3, .. random, .. TlsBytes.Vector(session, 1), 0x13, 1, 0, .. TlsBytes.Vector(fields)]);
    }

    private byte[] CreateServerFlight()
    {
        using var flight = new MemoryStream();
        void Send(byte type, byte[] body) => flight.Write(Record(TlsBytes.Message(type, body)));

        byte[] extensions = [.. TlsBytes.Extension(16, [0, 3, 2, (byte)'h', (byte)'3']), .. TlsBytes.Extension(57, LocalParameters)];
        Send(8, TlsBytes.Vector(extensions)); // EncryptedExtensions
        byte[] leaf = [.. TlsBytes.Vector(certificate.RawData, 3), 0, 0];
        Send(11, [0, .. TlsBytes.Vector(leaf, 3)]); // Certificate

        using var key = certificate.GetECDsaPrivateKey() ?? throw new CryptographicException("Expected an ECDSA private key.");
        var signature = key.SignData(CertificateVerifyInput(), HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        Send(15, [4, 3, .. TlsBytes.Vector(signature)]); // CertificateVerify
        Send(20, Finished(ServerHandshakeTraffic));
        DeriveApplicationKeys();
        return flight.ToArray();
    }
}
