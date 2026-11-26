// 40_2022_HTTP3_QUIC_STREAMS: QUIC streams and flow control for HTTP/3.
using System.Text;

byte[] limits = [.. QuicData.Pair(4, QuicData.Int(4)), .. QuicData.Pair(5, QuicData.Int(4))];
var streams = new QuicStreams(new QuicParameters(limits));
streams.Send(0, "Hello!"u8.ToArray(), fin: true);
foreach (var frame in streams.Frames())
{
    Console.WriteLine($"Limited STREAM frame: {Convert.ToHexString(frame)}");
}

streams.DataLimit = 64;
streams.Grant(0, 64);
foreach (var frame in streams.Frames())
{
    Console.WriteLine($"After MAX_DATA/MAX_STREAM_DATA: {Convert.ToHexString(frame)}");
}

streams.Receive(0, 3, "lo!"u8.ToArray(), fin: true);
streams.Receive(0, 0, "Hel"u8.ToArray(), fin: false);
Console.WriteLine($"Receive stream 0: {Encoding.ASCII.GetString(streams.Input[0].Peek())}; FIN: {streams.Input[0].Finished}");
