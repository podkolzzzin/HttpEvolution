using System.Security.Cryptography;

internal static partial class TlsCertificate
{
    public static string Pin => Convert.ToBase64String(SHA256.HashData(Key.ExportSubjectPublicKeyInfo()));
}
