using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Http3Streams.Handshake;

internal sealed class ClientHandshake(string fingerprint, byte[] parameters) : QuicTls(parameters)
{
    private enum Stage
    {
        Start, ServerHello, Extensions, Certificate, CertificateVerify, Finished, Established
    }

    private Stage stage;
    private X509Certificate2? certificate;

    public override byte[] StartClientHello()
    {
        if (stage != Stage.Start)
        {
            throw new InvalidOperationException("ClientHello already sent.");
        }

        byte[] keyShare = [0, 23, .. TlsBytes.Vector(PublicShare())]; // secp256r1
        byte[] name = [0, .. TlsBytes.Vector("localhost"u8.ToArray())];
        byte[] extensions = [
            .. TlsBytes.Extension(0, TlsBytes.Vector(name)),
            .. TlsBytes.Extension(43, [2, 3, 4]),       // TLS 1.3
            .. TlsBytes.Extension(13, [0, 2, 4, 3]),    // ecdsa_secp256r1_sha256
            .. TlsBytes.Extension(10, [0, 2, 0, 23]),   // supported_groups
            .. TlsBytes.Extension(51, TlsBytes.Vector(keyShare)),
            .. TlsBytes.Extension(16, [0, 3, 2, (byte)'h', (byte)'3']),
            .. TlsBytes.Extension(57, LocalParameters)
        ];
        byte[] body = [3, 3, .. RandomNumberGenerator.GetBytes(32), 0, 0, 2, 0x13, 1, 1, 0, .. TlsBytes.Vector(extensions)];
        stage = Stage.ServerHello;
        return Record(TlsBytes.Message(1, body));
    }

    protected override IReadOnlyList<(int Level, byte[] Bytes)> Handle(int level, int type, TlsBytes body, byte[] message)
    {
        switch (stage, level, type)
        {
            case (Stage.ServerHello, 0, 2):
                var peerShare = ReadServerHello(body);
                Record(message);
                DeriveHandshakeKeys(peerShare);
                stage = Stage.Extensions;
                return [];
            case (Stage.Extensions, 1, 8):
                var extensions = body.ReadExtensions();
                body.End();
                byte[] selection = TlsBytes.Unwrap(TlsBytes.Unwrap(TlsBytes.Required(extensions, 16)), 1);
                if (!selection.AsSpan().SequenceEqual("h3"u8))
                {
                    throw new InvalidDataException("Server did not select h3.");
                }
                PeerParameters = TlsBytes.Required(extensions, 57);
                stage = Stage.Certificate;
                break;
            case (Stage.Certificate, 1, 11):
                ReadCertificate(body);
                stage = Stage.CertificateVerify;
                break;
            case (Stage.CertificateVerify, 1, 15):
                VerifySignature(body);
                stage = Stage.Finished;
                break;
            case (Stage.Finished, 1, 20):
                VerifyFinished(body, ServerHandshakeTraffic);
                Record(message);
                DeriveApplicationKeys();
                var reply = Record(TlsBytes.Message(20, Finished(ClientHandshakeTraffic)));
                stage = Stage.Established;
                Complete = true;
                return [(1, reply)];
            case (Stage.Established, 2, 4):
                return []; // NewSessionTicket. Each demo run starts a fresh connection.
            default:
                throw new InvalidDataException("Unexpected TLS message from server.");
        }
        Record(message);
        return [];
    }

    private static byte[] ReadServerHello(TlsBytes body)
    {
        if (body.ReadInt() != 0x0303)
        {
            throw new InvalidDataException("Invalid ServerHello legacy version.");
        }
        body.Read(32);
        if (body.ReadVector(1).Length != 0 || body.ReadInt() != 0x1301 || body.ReadInt(1) != 0)
        {
            throw new InvalidDataException("Server selected unexpected session ID, cipher or compression.");
        }
        var fields = body.ReadExtensions();
        body.End();
        if (!TlsBytes.Required(fields, 43).AsSpan().SequenceEqual((byte[])[3, 4]))
        {
            throw new InvalidDataException("TLS 1.3 is required.");
        }

        var key = new TlsBytes(TlsBytes.Required(fields, 51));
        if (key.ReadInt() != 23)
        {
            throw new InvalidDataException("Expected a P-256 server key share.");
        }
        var share = key.ReadVector();
        key.End();
        return share;
    }

    private void ReadCertificate(TlsBytes body)
    {
        if (body.ReadVector(1).Length != 0)
        {
            throw new InvalidDataException("Unexpected certificate request context.");
        }

        var chain = new TlsBytes(body.ReadVector(3));
        body.End();
        certificate = X509CertificateLoader.LoadCertificate(chain.ReadVector(3));
        if (!string.Equals(certificate.GetCertHashString(HashAlgorithmName.SHA256), fingerprint, StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthenticationException("Server certificate fingerprint mismatch.");
        }
        chain.ReadExtensions();
        while (chain.Remaining > 0)
        {
            chain.ReadVector(3);
            chain.ReadExtensions();
        }
    }

    private void VerifySignature(TlsBytes body)
    {
        if (body.ReadInt() != 0x0403)
        {
            throw new InvalidDataException("Server selected an unsupported TLS signature scheme.");
        }

        byte[] signature = body.ReadVector();
        body.End();
        using var key = certificate!.GetECDsaPublicKey();
        if (key is null || !key.VerifyData(CertificateVerifyInput(), signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence))
        {
            throw new AuthenticationException("Invalid TLS CertificateVerify signature.");
        }
    }

    public override void Dispose()
    {
        certificate?.Dispose();
        base.Dispose();
    }
}
