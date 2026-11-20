// 22_2018_HTTP2_TLS13_KEYS: TLS1.3: ключі трафіку та захист записів.
using System.Security.Cryptography;
using System.Text;

var secret = TlsCrypto.Derive(new byte[32], "c hs traffic", "ClientHello + ServerHello"u8.ToArray());
using var cipher = new TlsCipher(secret);
var encrypted = cipher.Seal(0, "Hello, TLS1.3!"u8.ToArray(), [23, 3, 3]);
Console.WriteLine($"HKDF traffic secret: {Convert.ToHexString(secret)}");
Console.WriteLine($"AES-GCM ciphertext + tag: {Convert.ToHexString(encrypted)}");
Console.WriteLine($"Decrypted: {Encoding.ASCII.GetString(cipher.Open(0, encrypted, [23, 3, 3]))}");
CryptographicOperations.ZeroMemory(secret);
