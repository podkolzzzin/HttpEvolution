using System.Collections;
using Http3Streams.Quic;

namespace Http3Streams.Network;

// A stream offset names a byte, independent of which UDP datagram carried it.
internal sealed class ReceiveBuffer
{
    private byte[] bytes = [];
    private readonly BitArray received = new(0);
    private int contiguous;
    private int consumed;
    private int highest;
    private int? finalSize;

    public bool AtEnd => consumed == finalSize;

    public void Insert(long offset, ReadOnlySpan<byte> data, bool fin)
    {
        if (offset < 0 || offset > TransportParameters.StreamWindow - data.Length)
        {
            throw new InvalidDataException("STREAM data exceeds the advertised receive window.");
        }

        int start = (int)offset;
        int end = start + data.Length;
        if (end > finalSize || (fin && (end < highest || (finalSize.HasValue && end != finalSize))))
        {
            throw new InvalidDataException("Inconsistent QUIC stream final size.");
        }

        if (fin)
        {
            finalSize = end;
        }

        highest = Math.Max(highest, end);
        if (data.Length == 0)
        {
            return;
        }

        if (end > bytes.Length)
        {
            int size = Math.Min(TransportParameters.StreamWindow, Math.Max(end, Math.Max(256, 2 * bytes.Length)));
            Array.Resize(ref bytes, size);
            received.Length = size;
        }

        for (int i = start; i < end; i++)
        {
            if (received[i] && bytes[i] != data[i - start])
            {
                throw new InvalidDataException("Overlapping STREAM data differs.");
            }
            bytes[i] = data[i - start];
            received[i] = true;
        }

        while (contiguous < highest && contiguous < received.Length && received[contiguous])
        {
            contiguous++;
        }
    }

    public int Read(Span<byte> destination)
    {
        int count = Math.Min(destination.Length, contiguous - consumed);
        bytes.AsSpan(consumed, count).CopyTo(destination);
        consumed += count;
        return count;
    }
}
