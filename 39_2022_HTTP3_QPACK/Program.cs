// 39_2022_HTTP3_QPACK: HTTP/3 header compression and blocked decoding.
var bytes = Qpack.Encode(new(":method", "GET"), new(":path", "/"), new(":scheme", "https"));
Console.WriteLine($"QPACK field section: {Convert.ToHexString(bytes)}");
foreach (var (name, value) in Qpack.Decode(bytes))
{
    Console.WriteLine($"{name}: {value}");
}

Console.WriteLine();
ShowBlockedDecoding();

static void ShowBlockedDecoding()
{
    var table = new QpackDynamicTable(maxCapacity: 128);

    // Encoded Insert Count = 2 (one insert), Delta Base = 0, dynamic relative index = 0.
    byte[] headers = [0x02, 0x00, 0x80];
    var section = Qpack.ReadFieldSection(headers, table);
    if (section.RequiredInsertCount <= table.InsertCount)
    {
        throw new InvalidOperationException("Expected stream 0 to block.");
    }

    Console.WriteLine($"Stream 0 · HEADERS {Convert.ToHexString(headers)} · required inserts: {section.RequiredInsertCount}, received: {table.InsertCount} · waiting");
    var independent = Qpack.Decode([0x00, 0x00, 0xD1]).Single(); // Static index 17.
    Console.WriteLine($"Stream 4 · {independent.Name}: {independent.Value} · decoded");

    // Capacity = 128, then Insert With Literal Name: x-demo: ready.
    byte[] encoder = [0x3F, 0x61, 0x46, .. "x-demo"u8, 0x05, .. "ready"u8];
    var consumed = Qpack.ReadEncoderInstructions(encoder, table);
    if (consumed != encoder.Length || table.InsertCount != section.RequiredInsertCount)
    {
        throw new InvalidOperationException("Expected one complete encoder insertion.");
    }

    Console.WriteLine($"Encoder stream · {Convert.ToHexString(encoder)} · received inserts: {table.InsertCount}");
    var field = Qpack.Decode(section, table).Single();
    if (field != new HeaderField("x-demo", "ready"))
    {
        throw new InvalidOperationException("Expected the newly inserted field.");
    }

    Console.WriteLine($"Stream 0 · {field.Name}: {field.Value} · decoded");
}
