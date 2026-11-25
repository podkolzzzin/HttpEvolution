using System.Security.Cryptography;
using System.Text;

namespace Http3Streams.Handshake;

// TLS 1.3's HKDF-Expand-Label also derives QUIC packet keys (RFC 9001 §5.1).
internal static class TlsHkdf
{
    public static byte[] ExpandLabel(byte[] secret, string label, byte[] context, int length = 32)
    {
        byte[] name = Encoding.ASCII.GetBytes("tls13 " + label);
        byte[] info = [(byte)(length >> 8), (byte)length, .. TlsBytes.Vector(name, 1), .. TlsBytes.Vector(context, 1)];
        return HKDF.Expand(HashAlgorithmName.SHA256, secret, length, info);
    }
}
