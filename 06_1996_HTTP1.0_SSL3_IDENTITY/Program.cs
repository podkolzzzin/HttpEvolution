// 06_1996_HTTP1.0_SSL3_IDENTITY: SSL3: сертифікат RSA/SHA-1.
using System.Security.Cryptography.X509Certificates;

using var certificate = X509CertificateLoader.LoadCertificate(LegacyIdentity.Der);
Console.WriteLine($"Subject: {certificate.Subject}");
Console.WriteLine($"Signature: {certificate.SignatureAlgorithm.FriendlyName}");
Console.WriteLine($"SPKI pin: {LegacyIdentity.Pin}");
