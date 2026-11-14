using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

static partial class LegacySsl
{
    public static TlsStream Connect(Stream transport, string pin)
    {
        var records = new Ssl3Records(transport); byte[] master = [], premaster = [];
        try
        {
            if (transport.CanTimeout) transport.ReadTimeout = transport.WriteTimeout = 5000;
            var exchange = new Exchange(records); var client = HelloRandom();
            exchange.Send(1, [3, 0, .. client, 0, 0, 2, 0, 10, 1, 0]);
            var hello = new Wire(exchange.Read(2));
            if (hello.Int() != 0x0300) throw new IOException("Expected SSL 3.0.");
            var server = hello.Take(32); hello.Vector(1);
            if (hello.Int() != 10 || hello.Int(1) != 0) throw new IOException("Unexpected SSL cipher.");
            hello.End();
            var certificates = new Wire(exchange.Read(11)); var chain = new Wire(certificates.Vector(3)); certificates.End();
            using var certificate = X509CertificateLoader.LoadCertificate(chain.Vector(3)); chain.End();
            using var key = certificate.GetRSAPublicKey() ?? throw new IOException("Expected RSA certificate.");
            if (Convert.ToBase64String(SHA256.HashData(key.ExportSubjectPublicKeyInfo())) != pin) throw new IOException("Certificate SPKI pin mismatch.");
            if (exchange.Read(14).Length != 0) throw new IOException("Invalid ServerHelloDone.");
            premaster = RandomNumberGenerator.GetBytes(48); premaster[0] = 3; premaster[1] = 0;
            exchange.Send(16, key.Encrypt(premaster, RSAEncryptionPadding.Pkcs1));
            master = Ssl3Crypto.Expand(premaster, [.. client, .. server], 48); Keys(records, master, client, server, false);
            records.WriteChange(); exchange.Send(20, Ssl3Crypto.Finished(master, exchange.Transcript, true));
            records.ReadChange(); exchange.Verify(master, false);
            return new TlsStream(records);
        }
        catch { records.Dispose(); throw; }
        finally { CryptographicOperations.ZeroMemory(master); CryptographicOperations.ZeroMemory(premaster); }
    }
    public static void Client(string pin)
    {
        using var peer = new TcpClient(); peer.Connect(IPAddress.Loopback, 8442);
        using var stream = Connect(peer.GetStream(), pin);
        stream.Write("GET / HTTP/1.0\r\n\r\n"u8);
        using var reader = new StreamReader(stream, Encoding.ASCII); Console.Write(reader.ReadToEnd());
    }
}
