// 30_2021_HTTP3_QUIC_KEYS: QUIC v1: захист пакетів і заголовків.
using System.Text;

byte[] destination = Convert.FromHexString("8394c8f03e515708");
using var keys = new QuicKeys(QuicKeys.Initial(destination, server: false));
byte[] header = [0x43, .. destination, 0, 0, 0, 0];
var packet = keys.Seal(header, 9, 7, "Protected QUIC payload"u8.ToArray());
long largest = -1;
var decoded = keys.Open(packet, 9, ref largest);
Console.WriteLine($"Protected packet: {Convert.ToHexString(packet)}");
Console.WriteLine($"Packet number: {decoded.Number}; data: {Encoding.ASCII.GetString(decoded.Data)}");
