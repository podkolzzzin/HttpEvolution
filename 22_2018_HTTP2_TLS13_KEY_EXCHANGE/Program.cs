// A ClientHello without a P-256 key share prompts a retry.
var retryHandshake = new TlsHandshake("http/1.1");
var retry = retryHandshake.AcceptHello(DemoClientHello.Create(includeKeyShare: false));
Console.WriteLine($"HelloRetryRequest: {Convert.ToHexString(retry)}");

// A new handshake starts with a usable share and derives both traffic secrets.
var handshake = new TlsHandshake("http/1.1");
var serverHello = handshake.AcceptHello(DemoClientHello.Create());
Console.WriteLine($"ServerHello: {serverHello.Length} bytes, ALPN: {handshake.Protocol}");
Console.WriteLine($"Client handshake traffic secret: {Convert.ToHexString(handshake.ClientHandshakeTraffic)}");
Console.WriteLine($"Server handshake traffic secret: {Convert.ToHexString(handshake.ServerHandshakeTraffic)}");

// Protect one example handshake message with the server secret, record sequence number 0.
using var cipher = new TlsCipher(handshake.ServerHandshakeTraffic);
var extensions = Wire.Extension(16, Wire.Vector(Wire.Vector("http/1.1"u8.ToArray(), 1)));
byte[] innerPlaintext = [.. Wire.Message(8, Wire.Vector(extensions)), 22];
byte[] recordHeader = [23, 3, 3, .. Wire.Size(innerPlaintext.Length + 16)];
var encrypted = cipher.Seal(0, innerPlaintext, recordHeader);
Console.WriteLine($"AES-GCM ciphertext + tag: {Convert.ToHexString(encrypted)}");
Console.WriteLine($"Decrypted EncryptedExtensions + inner type: {Convert.ToHexString(cipher.Open(0, encrypted, recordHeader))}");
