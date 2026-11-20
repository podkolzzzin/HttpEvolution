// 34_2021_HTTP3_QUIC_ACK: QUIC: підтвердження діапазонів пакетів.
var bytes = QuicAcks.Encode([1, 2, 3, 7, 8, 11]);
Console.WriteLine($"ACK frame: {Convert.ToHexString(bytes)}");
var input = new QuicData(bytes);
Console.WriteLine($"Frame type: {input.Int()}");
foreach (var (low, high) in QuicAcks.Read(input, ecn: false))
    Console.WriteLine($"Acknowledged: {low}..{high}");
