// 39_2022_HTTP3_QPACK: HTTP/3: статичне кодування заголовків QPACK.
var bytes = Qpack.Encode((":method", "GET"), (":path", "/"), (":scheme", "https"));
Console.WriteLine($"QPACK field section: {Convert.ToHexString(bytes)}");
foreach (var (name, value) in Qpack.Decode(bytes))
    Console.WriteLine($"{name}: {value}");
