// 23_2018_HTTP2_TLS13_HELLO: TLS1.3: ClientHello та P-256 key share.
var hello = new TlsHello(DemoClientHello.Create(includeKeyShare: false));
Console.WriteLine($"ALPN: {string.Join(", ", hello.Protocols)}");
Console.WriteLine($"P-256 key share: {hello.Share.Length} bytes");
var retry = hello.Reply(null); // No server share requests a P-256 retry.
Console.WriteLine($"HelloRetryRequest: {Convert.ToHexString(retry)}");
