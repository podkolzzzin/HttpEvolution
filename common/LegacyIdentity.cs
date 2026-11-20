using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

static class LegacyIdentity
{
    sealed class Sha1Signer(RSA key) : X509SignatureGenerator
    {
        protected override PublicKey BuildPublicKey() =>
            X509SignatureGenerator.CreateForRSA(key, RSASignaturePadding.Pkcs1).PublicKey;
        public override byte[] GetSignatureAlgorithmIdentifier(HashAlgorithmName algorithm) => Convert.FromHexString("300d06092a864886f70d0101050500");
        public override byte[] SignData(byte[] data, HashAlgorithmName algorithm) =>
            key.SignData(data, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
    }

    static readonly RSA Key = RSA.Create(2048);
    public static byte[] Der { get; } = Create();
    public static string Pin => Convert.ToBase64String(SHA256.HashData(Key.ExportSubjectPublicKeyInfo()));

    static byte[] Create()
    {
        var request = new CertificateRequest("CN=localhost", Key, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName("localhost");
        names.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(names.Build());
        using var certificate = request.Create(
            request.SubjectName,
            new Sha1Signer(Key),
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow.AddDays(7),
            RandomNumberGenerator.GetBytes(16)
        );
        return certificate.RawData;
    }

    public static byte[] Premaster(byte[] encrypted, byte minor = 0)
    {
        var result = RandomNumberGenerator.GetBytes(48);
        try
        {
            var decoded = Key.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);
            if (decoded.Length == 48 && decoded[0] == 3 && decoded[1] == minor)
            {
                decoded.CopyTo(result, 0);
            }

            CryptographicOperations.ZeroMemory(decoded);
        }
        catch (CryptographicException)
        {
        } // Invalid RSA padding/version fails later at Finished.

        return result;
    }
}
