// A small loopback recovery model: ACK ranges, packet-threshold loss and backed-off PTO.
internal sealed class QuicRecovery
{
    public sealed class Frame(int level, byte[] bytes, bool reliable)
    {
        public readonly int Level = level;
        public readonly byte[] Bytes = bytes;
        public readonly bool Reliable = reliable;
        public bool Done;
        public bool Lost;
        public long Last;
        public int Attempts;
        public ulong Number;
    }

    private sealed class Transmission(Frame frame, int size)
    {
        public readonly Frame Frame = frame;
        public int Size = size;
    }

    private readonly record struct PacketKey(int Level, ulong Number);

    private readonly List<Frame> pending = [];
    private readonly Dictionary<PacketKey, Transmission> sent = [];
    private long nextProbe;
    public int Window
    {
        get;
        private set;
    } = 12000;
    public int InFlight => sent.Values.Sum(packet => packet.Size);

    private static long Delay(Frame frame) => 1000L << Math.Min(Math.Max(0, frame.Attempts - 1), 3);
    public void Add(int level, byte[] bytes, bool reliable = true)
    {
        if (pending.Count >= 256)
        {
            throw new InvalidDataException("Too many pending QUIC frames.");
        }

        pending.Add(new(level, bytes, reliable));
    }

    public IEnumerable<Frame> Ready(int level, long now) =>
        pending.Where(frame => !frame.Done && frame.Level == level
                && (frame.Attempts == 0
                || (now >= nextProbe
                && (frame.Lost || now - frame.Last >= Delay(frame)))));
    public bool CanSend(Frame frame, int size) => !frame.Reliable || frame.Attempts != 0 || InFlight + size <= Window;
    public void Record(Frame frame, ulong number, int size, long now)
    {
        if (frame.Attempts >= 8)
        {
            throw new InvalidDataException("QUIC retry limit.");
        }

        if (frame.Attempts != 0)
        {
            foreach (var packet in sent.Values.Where(packet => packet.Frame == frame))
            {
                packet.Size = 0;
            }

            Window = Math.Max(2400, Window / 2);
            nextProbe = now + Math.Min(8000, Delay(frame) * 2);
        }

        frame.Last = now;
        frame.Number = number;
        frame.Attempts++;
        frame.Lost = false;
        if (frame.Reliable)
        {
            sent.Add(new(frame.Level, number), new(frame, size));
        }
        else
        {
            frame.Done = true;
        }

        Clean();
    }

    public void Acknowledge(int level, List<QuicAckRange> ranges, ulong next)
    {
        if (ranges.Any(range => range.High >= next))
        {
            throw new InvalidDataException("ACK for an unsent packet.");
        }

        if (ranges.Count == 0)
        {
            return;
        }

        var largest = ranges.Max(range => range.High);
        bool progress = false;
        foreach (var (key, packet) in sent)
        {
            if (key.Level != level)
            {
                continue;
            }

            if (ranges.Any(range => key.Number >= range.Low && key.Number <= range.High))
            {
                packet.Frame.Done = true;
                progress = true;
            }
            else if (key.Number + 3 <= largest && key.Number == packet.Frame.Number)
            {
                packet.Frame.Lost = true;
            }
        }

        if (progress)
        {
            nextProbe = 0;
            Window = Math.Min(12000, Window + 1200);
        }

        Clean();
    }

    private void Clean()
    {
        pending.RemoveAll(frame => frame.Done);
        foreach (var key in sent.Where(pair => pair.Value.Frame.Done).Select(pair => pair.Key).ToArray())
        {
            sent.Remove(key);
        }
    }

    public void Discard(int level)
    {
        foreach (var frame in pending.Where(frame => frame.Level == level))
        {
            frame.Done = true;
        }

        Clean();
    }

    public void Clear()
    {
        pending.Clear();
        sent.Clear();
    }
}
