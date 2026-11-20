// 04_1996_HTTP1.0_SSL3_RECORDS: SSL3: MAC і шифрування записів 3DES-CBC.
using System.Text;

byte[] mac = Enumerable.Range(1, 20).Select(i => (byte)i).ToArray();
byte[] key = Enumerable.Range(1, 24).Select(i => (byte)i).ToArray();
using var sender = new LegacyCipher(mac, key, new byte[8]);
using var receiver = new LegacyCipher(mac, key, new byte[8]);
var encrypted = sender.Seal(23, "GET / HTTP/1.0\r\n\r\n"u8.ToArray());
Console.WriteLine($"SSL3 MAC + 3DES-CBC: {Convert.ToHexString(encrypted)}");
Console.WriteLine($"Decrypted: {Encoding.ASCII.GetString(receiver.Open(23, encrypted))}");
