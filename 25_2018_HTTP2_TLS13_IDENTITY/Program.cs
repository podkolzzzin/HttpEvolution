// 25_2018_HTTP2_TLS13_IDENTITY: TLS1.3: сертифікат localhost і підписи ECDSA.
using System.Security.Cryptography.X509Certificates;

using var certificate = X509CertificateLoader.LoadCertificate(TlsCertificate.Der);
Console.WriteLine($"Subject: {certificate.Subject}");
Console.WriteLine($"Signature: {certificate.SignatureAlgorithm.FriendlyName}");
var signature = TlsCertificate.Sign("Demo TLS transcript"u8.ToArray());
Console.WriteLine($"ECDSA CertificateVerify: {Convert.ToHexString(signature)}");
