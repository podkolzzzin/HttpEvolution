// 38_2021_HTTP3_QUIC_PTO: QUIC ACK tracking and probe timeout.
var recovery = new QuicRecovery();
recovery.Add(0, [1]); // Reliable PING for a synthetic clock, without waiting.
var frame = recovery.Ready(0, 0).Single();
recovery.Record(frame, number: 0, size: 1200, now: 0);
Console.WriteLine($"In flight: {recovery.InFlight}; ready at 999 ms: {recovery.Ready(0, 999).Count()}");
recovery.Record(recovery.Ready(0, 1000).Single(), number: 1, size: 1200, now: 1000);
Console.WriteLine($"PTO retransmission: packet {frame.Number}; attempts {frame.Attempts}; window {recovery.Window}");
recovery.Acknowledge(0, [new(1, 1)], next: 2);
Console.WriteLine($"After ACK: in flight {recovery.InFlight}");
