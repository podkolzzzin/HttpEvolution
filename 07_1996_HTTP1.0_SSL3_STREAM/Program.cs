// 07_1996_HTTP1.0_SSL3_STREAM: SSL3: розшифровані записи як потік байтів.
using var wire = new MemoryStream();
using var records = new LegacyRecords(wire);
records.Write(23, "HTTP/1.0 200 OK\r\n\r\nHello through TlsStream!\n"u8.ToArray());
records.Write(21, [1, 0]); // close_notify ends the application byte stream.
wire.Position = 0;
using var stream = new TlsStream(new LegacyRecords(wire));
using var reader = new StreamReader(stream);
Console.Write(reader.ReadToEnd());
