// 05_1996_HTTP1.0_SSL3_MESSAGES: SSL3: повідомлення handshake та ChangeCipherSpec.
using var wire = new MemoryStream();
using var writer = new LegacyRecords(wire);
writer.Write(22, Wire.Message(14, [])); // ServerHelloDone, framed as an SSL3 record.
Console.WriteLine($"Record bytes: {Convert.ToHexString(wire.ToArray())}");
wire.Position = 0;
using var reader = new LegacyRecords(wire);
Console.WriteLine($"Handshake bytes: {Convert.ToHexString(reader.Handshake())}");
