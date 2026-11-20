// 33_2021_HTTP3_QUIC_OFFSETS: QUIC: збирання фрагментів потоків за offset.
using System.Text;

var buffer = new QuicBuffer();
buffer.Add(5, "QUIC!"u8.ToArray(), fin: true);
Console.WriteLine($"Tail arrived first; available: {buffer.Available}");
buffer.Add(0, "Hello"u8.ToArray());
Console.WriteLine($"Reassembled: {Encoding.ASCII.GetString(buffer.Peek())}; FIN: {buffer.Finished}");
