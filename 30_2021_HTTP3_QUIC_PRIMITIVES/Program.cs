using System.Text;

byte[] destinationId = QuicConstants.DemoDestinationConnectionId;
byte[] clientId = QuicConstants.DemoSourceConnectionId;

ShowVarInts();
ShowInitialPacket(destinationId, clientId);
ShowStreamReassembly();
ShowAckRanges();
ShowTransportParameters(destinationId, clientId);

static void ShowVarInts()
{
    Console.WriteLine("QUIC varint");
    ulong[] values = [37, 15293, 494878333, 151288809941952652];
    foreach (var value in values)
    {
        var encoded = QuicData.Int(value);
        Console.WriteLine($"{value} -> {Convert.ToHexString(encoded)} ({encoded.Length} B) -> {new QuicData(encoded).Int()}");
    }
}

static void ShowInitialPacket(byte[] destinationId, byte[] clientId)
{
    Console.WriteLine("\nInitial: packet header and protection");
    using var keys = new QuicKeys(QuicKeys.Initial(destinationId, server: false));
    const byte ping = 0x01;
    var datagram = QuicPacket.Long(0, destinationId, clientId, 7, [ping], keys);
    var packet = QuicPacket.Read(datagram);
    Console.WriteLine($"QUIC v{packet.Version}, {datagram.Length} B, DCID {Convert.ToHexString(packet.Destination)}, SCID {Convert.ToHexString(packet.Source)}");
    Console.WriteLine($"First byte of the protected header: {datagram[0]:X2}");
    long largestReceived = -1;
    var opened = keys.Open(packet.Bytes, packet.NumberOffset, ref largestReceived);
    Console.WriteLine($"After removing protection: flags {opened.Flags:X2}, packet {opened.Number}, frame {opened.Data[0]:X2} (PING)");
}

static void ShowStreamReassembly()
{
    Console.WriteLine("\nStream: reassembling fragments by offset");
    var stream = new QuicBuffer();
    stream.Add(5, "QUIC!"u8.ToArray(), fin: true);
    Console.WriteLine($"Bytes starting at offset 5 arrive first. Available to read: {stream.Available} B");
    stream.Add(0, "Hello"u8.ToArray());
    Console.WriteLine($"After receiving the start of the stream: {Encoding.ASCII.GetString(stream.Peek())}, FIN={stream.Finished}");
}

static void ShowAckRanges()
{
    Console.WriteLine("\nACK: received packets 1, 2, 3, 7, 8, 11");
    var ack = QuicAcks.Encode([1, 2, 3, 7, 8, 11]);
    var input = new QuicData(ack);
    Console.WriteLine($"Frame {Convert.ToHexString(ack)}, type {input.Int():X2}");
    foreach (var (low, high) in QuicAcks.Read(input, ecn: false))
    {
        Console.WriteLine($"Acknowledged {low}..{high}");
    }
}

static void ShowTransportParameters(byte[] destinationId, byte[] clientId)
{
    Console.WriteLine("\nTransport parameters");
    byte[] clientBytes = [.. QuicData.Pair(15, clientId), .. QuicData.Pair(4, QuicData.Int(1_048_576))];
    var client = new QuicParameters(clientBytes);
    client.ValidateClient(clientId);
    Console.WriteLine($"Client SCID verified, initial_max_data={client.Value(4)}");
    byte[] serverId = Convert.FromHexString("0203040506070809");
    var server = new QuicParameters(QuicParameters.Server(destinationId, serverId));
    Console.WriteLine($"Server: initial_max_data={server.Value(4)}, initial_max_streams_bidi={server.Value(8)}, max_udp_payload_size={server.Value(3)}");
}
