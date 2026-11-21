using System.Security.Cryptography.X509Certificates;
using System.Text;

using var handshake = new TlsHandshake("http/1.1");
handshake.AcceptHello(DemoClientHello.Create());

using var certificate = X509CertificateLoader.LoadCertificate(TlsCertificate.Der);
Console.WriteLine($"Certificate: {certificate.Subject}, {certificate.SignatureAlgorithm.FriendlyName}");
foreach (var message in handshake.ServerFlight())
{
    var name = message[0] switch
    {
        8 => "EncryptedExtensions",
        11 => "Certificate",
        15 => "CertificateVerify",
        20 => "Finished",
        _ => throw new InvalidDataException("Unexpected handshake message.")
    };
    Console.WriteLine($"Server {name}: {message.Length} bytes");
}

// Both endpoints run in memory, so the demo also computes the client's Finished.
var finished = TlsCrypto.Finished(handshake.ClientHandshakeTraffic, handshake.Transcript);
handshake.VerifyFinished(Wire.Message(20, finished));
Console.WriteLine($"Client Finished verified: {handshake.Complete}");

using var wire = new MemoryStream();
using var server = new TlsRecords(wire);
server.Keys(handshake.ClientTraffic, handshake.ServerTraffic);
server.Write(23, "Hello inside a TLS1.3 record!"u8.ToArray());
Console.WriteLine($"Encrypted record: {Convert.ToHexString(wire.ToArray())}");

// TlsRecords.Keys takes incoming, then outgoing secrets. The client reverses them.
wire.Position = 0;
using var client = new TlsRecords(wire);
client.Keys(handshake.ServerTraffic, handshake.ClientTraffic);
var (type, data) = client.Read();
Console.WriteLine($"Inner content type: {type}, data: {Encoding.UTF8.GetString(data)}");
