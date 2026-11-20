// 27_2018_HTTP2_TLS13_RECORDS: TLS1.3: формат і автентифікація записів.
using System.Text;

using var wire = new MemoryStream();
using var writer = new TlsRecords(wire);
writer.Keys(new byte[32], new byte[32]); // Matching demo secrets for both directions.
writer.Write(23, "Hello inside a TLS1.3 record!"u8.ToArray());
Console.WriteLine($"Encrypted record: {Convert.ToHexString(wire.ToArray())}");
wire.Position = 0;
using var reader = new TlsRecords(wire);
reader.Keys(new byte[32], new byte[32]);
var (type, data) = reader.Read();
Console.WriteLine($"Inner content type: {type}; data: {Encoding.ASCII.GetString(data)}");
