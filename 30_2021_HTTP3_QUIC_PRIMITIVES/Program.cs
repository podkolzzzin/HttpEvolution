using System.Text;

byte[] destinationId = QuicConstants.DemoDestinationConnectionId;
byte[] clientId = QuicConstants.DemoSourceConnectionId;

Console.WriteLine("QUIC varint");
ulong[] values = [37, 15293, 494878333, 151288809941952652];
foreach (var value in values)
{
    var encoded = QuicData.Int(value);
    Console.WriteLine($"{value} -> {Convert.ToHexString(encoded)} ({encoded.Length} B) -> {new QuicData(encoded).Int()}");
}

Console.WriteLine("\nInitial: заголовок і захист пакета");
using var keys = new QuicKeys(QuicKeys.Initial(destinationId, server: false));
const byte ping = 0x01;
var datagram = QuicPacket.Long(0, destinationId, clientId, 7, [ping], keys);
var packet = QuicPacket.Read(datagram);
Console.WriteLine($"QUIC v{packet.Version}, {datagram.Length} B, DCID {Convert.ToHexString(packet.Destination)}, SCID {Convert.ToHexString(packet.Source)}");
Console.WriteLine($"Перший байт захищеного заголовка: {datagram[0]:X2}");
long largestReceived = -1;
var opened = keys.Open(packet.Bytes, packet.NumberOffset, ref largestReceived);
Console.WriteLine($"Після зняття захисту: {opened.Flags:X2}, номер {opened.Number}, кадр {opened.Data[0]:X2} (PING)");

Console.WriteLine("\nПотік: складання фрагментів за зміщенням");
var stream = new QuicBuffer();
stream.Add(5, "QUIC!"u8.ToArray(), fin: true);
Console.WriteLine($"Спочатку отримано байти від зміщення 5. Доступно для читання: {stream.Available} B");
stream.Add(0, "Hello"u8.ToArray());
Console.WriteLine($"Після отримання початку: {Encoding.ASCII.GetString(stream.Peek())}, FIN={stream.Finished}");

Console.WriteLine("\nACK: отримані пакети 1, 2, 3, 7, 8, 11");
var ack = QuicAcks.Encode([1, 2, 3, 7, 8, 11]);
var input = new QuicData(ack);
Console.WriteLine($"Кадр {Convert.ToHexString(ack)}, тип {input.Int():X2}");
foreach (var (low, high) in QuicAcks.Read(input, ecn: false))
{
    Console.WriteLine($"Підтверджено {low}..{high}");
}

Console.WriteLine("\nТранспортні параметри");
byte[] clientBytes = [.. QuicData.Pair(15, clientId), .. QuicData.Pair(4, QuicData.Int(1_048_576))];
var client = new QuicParameters(clientBytes);
client.ValidateClient(clientId);
Console.WriteLine($"SCID клієнта підтверджено, initial_max_data={client.Value(4)}");
byte[] serverId = Convert.FromHexString("0203040506070809");
var server = new QuicParameters(QuicParameters.Server(destinationId, serverId));
Console.WriteLine($"Сервер: initial_max_data={server.Value(4)}, initial_max_streams_bidi={server.Value(8)}, max_udp_payload_size={server.Value(3)}");
