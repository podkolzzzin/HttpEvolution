namespace Http3Streams.Quic;

// CRYPTO offsets allow reordered fragments and retransmissions.
internal sealed class CryptoBuffer(int capacity = 65536)
{
    private readonly byte[] data = new byte[capacity];
    private readonly bool[] present = new bool[capacity];
    private int contiguous;
    private int consumed;
    private int highest;
    public int Available => contiguous - consumed;

    public byte[] Peek() => data[consumed..contiguous];
    public void Consume(int count)
    {
        if (count < 0 || count > Available)
        {
            throw new InvalidDataException("Read beyond contiguous stream data.");
        }

        consumed += count;
    }

    public void Add(ulong offset, byte[] bytes)
    {
        if (offset > (ulong)capacity || bytes.Length > capacity - (int)offset)
        {
            throw new InvalidDataException("QUIC receive buffer exceeded.");
        }

        var start = (int)offset;
        var end = start + bytes.Length;
        for (var i = 0; i < bytes.Length; i++)
        {
            if (present[start + i] && data[start + i] != bytes[i])
            {
                throw new InvalidDataException("Conflicting stream bytes.");
            }

            data[start + i] = bytes[i];
            present[start + i] = true;
        }

        highest = Math.Max(highest, end);
        while (contiguous < highest && present[contiguous])
        {
            contiguous++;
        }
    }

    public byte[]? Message()
    {
        if (Available < 4)
        {
            return null;
        }

        var length = (data[consumed + 1] << 16) | (data[consumed + 2] << 8) | data[consumed + 3];
        if (length > capacity - 4)
        {
            throw new InvalidDataException("CRYPTO handshake message too large.");
        }

        if (Available < length + 4)
        {
            return null;
        }

        var message = data[consumed..(consumed + length + 4)];
        Consume(length + 4);
        return message;
    }
}
