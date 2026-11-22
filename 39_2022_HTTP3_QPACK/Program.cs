// 39_2022_HTTP3_QPACK: стиснення заголовків HTTP/3 і блокування декодування.
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

    Console.WriteLine($"Потік 0 · HEADERS {Convert.ToHexString(headers)} · потрібно вставок: {section.RequiredInsertCount}, отримано: {table.InsertCount} · чекає");
    var independent = Qpack.Decode([0x00, 0x00, 0xD1]).Single(); // Static index 17.
    Console.WriteLine($"Потік 4 · {independent.Name}: {independent.Value} · декодовано");

    // Capacity = 128, then Insert With Literal Name: x-demo: ready.
    byte[] encoder = [0x3F, 0x61, 0x46, .. "x-demo"u8, 0x05, .. "ready"u8];
    var consumed = Qpack.ReadEncoderInstructions(encoder, table);
    if (consumed != encoder.Length || table.InsertCount != section.RequiredInsertCount)
    {
        throw new InvalidOperationException("Expected one complete encoder insertion.");
    }

    Console.WriteLine($"Encoder stream · {Convert.ToHexString(encoder)} · отримано вставок: {table.InsertCount}");
    var field = Qpack.Decode(section, table).Single();
    if (field != new HeaderField("x-demo", "ready"))
    {
        throw new InvalidOperationException("Expected the newly inserted field.");
    }

    Console.WriteLine($"Потік 0 · {field.Name}: {field.Value} · декодовано");
}
