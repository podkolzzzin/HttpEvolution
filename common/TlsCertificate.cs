using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
        byte[] context = [
            ..Enumerable.Repeat((byte)32, 64),
            ..Encoding.ASCII.GetBytes("TLS 1.3, server CertificateVerify\0"),
            ..SHA256.HashData(transcript)
        ];
        return Key.SignData(context, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
    }
}
