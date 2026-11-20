// 31_2021_HTTP3_QUIC_VARINT: QUIC: цілі числа змінної довжини.
foreach (var value in new ulong[] { 37, 15293, 494878333, 151288809941952652 })
{
    var encoded = QuicData.Int(value);
    Console.WriteLine($"{value} -> {Convert.ToHexString(encoded)} ({encoded.Length} bytes) -> {new QuicData(encoded).Int()}");
}
