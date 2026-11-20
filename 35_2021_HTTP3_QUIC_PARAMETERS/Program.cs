// 35_2021_HTTP3_QUIC_PARAMETERS: QUIC: транспортні параметри.
byte[] source = Convert.FromHexString("0102030405060708");
byte[] clientBytes = [.. QuicData.Pair(15, source), .. QuicData.Pair(4, QuicData.Int(1048576))];
var client = new QuicParameters(clientBytes);
client.ValidateClient(source);
var server = new QuicParameters(QuicParameters.Server(source, source));
Console.WriteLine($"Client CID validated; max_data: {client.Value(4)}");
Console.WriteLine($"Server max_data: {server.Value(4)}; bidi streams: {server.Value(8)}; UDP payload: {server.Value(3)}");
