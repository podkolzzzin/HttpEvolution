// 36_2021_HTTP3_QUIC_TLS: TLS handshake across QUIC encryption levels.
using System.Net;

byte[] destination = QuicConstants.DemoDestinationConnectionId;
byte[] source = QuicConstants.DemoSourceConnectionId;
var hello = DemoClientHello.Create(QuicStrings.Http3Alpn, QuicData.Pair(15, source));
byte[] crypto = [6, .. QuicData.Int(0), .. QuicData.Vector(hello)];
using var keys = new QuicKeys(QuicKeys.Initial(destination, server: false));
var initial = QuicPacket.Long(0, destination, source, 0, crypto, keys);
using var session = new QuicSession(new IPEndPoint(IPAddress.Loopback, 12345), destination, source);
session.Receive(QuicPacket.Read(initial)); // In-memory ClientHello -> server handshake flight.
foreach (var bytes in session.Flush())
{
    Console.WriteLine($"Server packet: level {QuicPacket.Read(bytes).Level}; {bytes.Length} bytes");
}
