// 17_2015_HTTP2_HPACK: HPACK: таблиці заголовків.
var hpack = new Hpack();
foreach (var (name, value) in hpack.Decode([0x82, 0x86, 0x84]))
{
    Console.WriteLine($"{name}: {value}");
}
