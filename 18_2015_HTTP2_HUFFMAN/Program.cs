// 18_2015_HTTP2_HUFFMAN: HPACK: декодування рядків Huffman.
var encoded = Convert.FromHexString("f1e3c2e5f23a6ba0ab90f4ff");
Console.WriteLine($"Huffman bytes: {Convert.ToHexString(encoded)}");
var hpack = new Hpack();
foreach (var (name, value) in hpack.Decode([0x01, 0x8c, .. encoded]))
{
    Console.WriteLine($"{name}: {value}");
}
