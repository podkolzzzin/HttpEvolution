// 24_2018_HTTP2_TLS13_ECDH: TLS1.3: узгодження секретів ECDH/P-256.
var handshake = new TlsHandshake("http/1.1");
var reply = handshake.AcceptHello(DemoClientHello.Create());
Console.WriteLine($"ServerHello: {reply.Length} bytes");
Console.WriteLine($"ALPN: {handshake.Protocol}");
Console.WriteLine($"Client handshake traffic secret: {Convert.ToHexString(handshake.ClientHandshakeTraffic)}");
Console.WriteLine($"Server handshake traffic secret: {Convert.ToHexString(handshake.ServerHandshakeTraffic)}");
