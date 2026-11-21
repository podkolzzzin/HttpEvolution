var hpack = new Hpack(Huffman.Decode);
var domain = DemoHpackBytes.HuffmanAuthority;

// 82 86 84 select static entries. 41 inserts :authority, 8c marks a 12-byte Huffman string.
byte[] firstRequest = [.. DemoHpackBytes.IndexedRequestHeaders, 0x41, 0x8c, .. domain];
PrintHeaders("First request", firstRequest);

// BE selects entry 62, the first entry of the dynamic table on this connection.
PrintHeaders("Repeated request", [.. DemoHpackBytes.IndexedRequestHeaders, 0xbe]);

void PrintHeaders(string label, byte[] block)
{
    Console.WriteLine($"{label} ({block.Length} bytes): {Convert.ToHexString(block)}");
    foreach (var (name, value) in hpack.Decode(block))
    {
        Console.WriteLine($"{name}: {value}");
    }
}
