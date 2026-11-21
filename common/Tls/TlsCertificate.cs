using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static partial class TlsCertificate
{
    private static readonly ECDsa Key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    public static byte[] Der { get; } = Create();

    private static byte[] Create()
    {
        var request = new CertificateRequest("CN=localhost", Key, HashAlgorithmName.SHA256);
        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName("localhost");
        names.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(names.Build());
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow.AddDays(7)
        );
        return certificate.RawData;
    }

    public static byte[] Sign(byte[] transcript)
    {
        const int paddingLength = 64;
        ReadOnlySpan<byte> label = "TLS 1.3, server CertificateVerify\0"u8;
        var context = new byte[paddingLength + label.Length + SHA256.HashSizeInBytes];
        context.AsSpan(0, paddingLength).Fill((byte)' ');
        label.CopyTo(context.AsSpan(paddingLength));
        SHA256.HashData(transcript, context.AsSpan(paddingLength + label.Length));
        return Key.SignData(context, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
    }
}
