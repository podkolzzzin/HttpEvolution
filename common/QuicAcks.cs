static class QuicAcks
{
    public static byte[] Encode(SortedSet<ulong> received)
    {
        var ranges = new List<(ulong Low, ulong High)>();
        foreach (var number in received.Reverse())
        {
            if (ranges.Count != 0 && number + 1 == ranges[^1].Low)
            {
                ranges[^1] = (number, ranges[^1].High);
            }
            else
            {
                if (ranges.Count == 64)
                {
                    break;
                }

                ranges.Add((number, number));
            }
        }

        if (ranges.Count == 0)
        {
            return [];
        }

        using var output = new MemoryStream();
        void Put(ulong value) => output.Write(QuicData.Int(value));
        Put(2);
        Put(ranges[0].High);
        Put(0);
        Put((ulong)ranges.Count - 1);
        Put(ranges[0].High - ranges[0].Low);
        for (var i = 1; i < ranges.Count; i++)
        {
            Put(ranges[i - 1].Low - ranges[i].High - 2);
            Put(ranges[i].High - ranges[i].Low);
        }

        return output.ToArray();
    }

    public static List<(ulong Low, ulong High)> Read(QuicData input, bool ecn)
    {
        var high = input.Int();
        input.Int(); // Largest acknowledged and ACK delay
        var count = input.Int();
        var length = input.Int();
        if (count > 256 || length > high)
        {
            throw new InvalidDataException("Invalid ACK range.");
        }

        var ranges = new List<(ulong Low, ulong High)>
        {
            (high - length, high)
        };
        for (ulong i = 0; i < count; i++)
        {
            var gap = input.Int() + 2;
            if (gap > ranges[^1].Low)
            {
                throw new InvalidDataException("ACK gap underflow.");
            }

            high = ranges[^1].Low - gap;
            length = input.Int();
            if (length > high)
            {
                throw new InvalidDataException("ACK range underflow.");
            }

            ranges.Add((high - length, high));
        }

        if (ecn)
        {
            input.Int();
            input.Int();
            input.Int();
        }

        return ranges;
    }
}
