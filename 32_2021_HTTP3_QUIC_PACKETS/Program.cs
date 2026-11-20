// 32_2021_HTTP3_QUIC_PACKETS: QUIC: формат пакетів і connection ID.
byte[] destination = Convert.FromHexString("8394c8f03e515708");
byte[] source = Convert.FromHexString("0102030405060708");
using var keys = new QuicKeys(QuicKeys.Initial(destination, server: false));
var initial = QuicPacket.Long(0, destination, source, 0, [1], keys); // PING + padding.
var packet = QuicPacket.Read(initial);
Console.WriteLine($"Initial datagram: {initial.Length} bytes; QUIC v{packet.Version}; level {packet.Level}");
Console.WriteLine($"Destination CID: {Convert.ToHexString(packet.Destination)}; source CID: {Convert.ToHexString(packet.Source)}");
