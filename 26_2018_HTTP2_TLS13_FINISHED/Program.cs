// 26_2018_HTTP2_TLS13_FINISHED: TLS1.3: автентифікація та Finished.
using var handshake = new TlsHandshake("http/1.1");
handshake.AcceptHello(DemoClientHello.Create());
foreach (var message in handshake.ServerFlight())
    Console.WriteLine($"Server handshake message {message[0]}: {message.Length} bytes");
// Emulate the client's Finished with the negotiated secret, without a network peer.
var finished = TlsCrypto.Finished(handshake.ClientHandshakeTraffic, handshake.Transcript);
handshake.VerifyFinished(Wire.Message(20, finished));
Console.WriteLine($"Finished verified: {handshake.Complete}");
