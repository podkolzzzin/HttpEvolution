using System.Security.Cryptography.X509Certificates;
using System.Text;

ShowCipher();
ShowHandshakeMessage();
ShowCertificate();
ShowApplicationStream();

static void ShowCipher()
{
    byte[] mac = Sequence(20);
    byte[] key = Sequence(24);
    using var sender = new LegacyCipher(mac, key, new byte[8]);
    using var receiver = new LegacyCipher(mac, key, new byte[8]);
    var encrypted = sender.Seal(23, "GET / HTTP/1.0\r\n\r\n"u8.ToArray());
    Console.WriteLine($"SSL3 MAC + 3DES-CBC: {Convert.ToHexString(encrypted)}");
    Console.WriteLine($"Decrypted: {Encoding.ASCII.GetString(receiver.Open(23, encrypted))}");
}

static void ShowHandshakeMessage()
{
    using var wire = new MemoryStream();
    using var writer = new LegacyRecords(wire);
    writer.Write(22, Wire.Message(14, [])); // ServerHelloDone, framed as an SSL3 record.
    Console.WriteLine($"Record bytes: {Convert.ToHexString(wire.ToArray())}");
    wire.Position = 0;
    using var reader = new LegacyRecords(wire);
    Console.WriteLine($"Handshake bytes: {Convert.ToHexString(reader.Handshake())}");
}

static void ShowCertificate()
{
    using var certificate = X509CertificateLoader.LoadCertificate(LegacyIdentity.Der);
    Console.WriteLine($"Subject: {certificate.Subject}");
    Console.WriteLine($"Signature: {certificate.SignatureAlgorithm.FriendlyName}");
    Console.WriteLine($"SPKI pin: {LegacyIdentity.Pin}");
}

static void ShowApplicationStream()
{
    using var wire = new MemoryStream();
    // The full handshake in step 08 derives this block instead.
    byte[] keyBlock = Sequence(104);
    using var records = new LegacyRecords(wire);
    records.Keys(keyBlock, server: true);
    records.WriteChange();
    records.Write(23, "HTTP/1.0 200 OK\r\n\r\nHello through TlsStream!\n"u8.ToArray());
    records.Write(21, [1, 0]); // close_notify ends the application byte stream.
    wire.Position = 0;
    using var client = new LegacyRecords(wire);
    client.Keys(keyBlock, server: false);
    client.ReadChange();
    using var stream = new TlsStream(client);
    using var reader = new StreamReader(stream);
    Console.Write(reader.ReadToEnd());
}

static byte[] Sequence(int length)
{
    var bytes = new byte[length];
    for (var i = 0; i < length; i++)
    {
        bytes[i] = (byte)(i + 1);
    }

    return bytes;
}
