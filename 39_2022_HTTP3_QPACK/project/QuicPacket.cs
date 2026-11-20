using System.Buffers.Binary;

sealed record QuicPacket(int Level, uint Version, byte[] Destination, byte[] Source, int NumberOffset, byte[] Bytes)
{
    public static QuicPacket Read(byte[] datagram, int shortIdLength = 8)
    {
        var input = new QuicData(datagram);
        var flags = input.Byte();
        if ((flags & 128) == 0)
        {
            if ((flags & 64) == 0) throw new InvalidDataException("Missing QUIC fixed bit.");
            var destination = input.Take(shortIdLength);
            return new(2, 1, destination, [], input.Offset, datagram);
        }
        var version = BinaryPrimitives.ReadUInt32BigEndian(input.Take(4));
        var dcid = input.Take(input.Byte()); var scid = input.Take(input.Byte());
        if (dcid.Length > 20 || scid.Length > 20) throw new InvalidDataException("QUIC connection ID too long.");
        if (version != 1) return new(-1, version, dcid, scid, 0, datagram);
        if ((flags & 64) == 0) throw new InvalidDataException("Missing QUIC fixed bit.");
        var type = (flags >> 4) & 3;
        if (type == 3) return new(-1, version, dcid, scid, 0, datagram); // Retry has no packet number.
        if (type == 0) input.Vector(); // Initial token
        var length = input.Int();
        if (length > (ulong)input.Left) throw new InvalidDataException("Truncated QUIC packet.");
        return new(type == 0 ? 0 : type == 2 ? 1 : -1, version, dcid, scid, input.Offset, datagram[..(input.Offset + (int)length)]);
    }
    public static byte[] Long(int level, byte[] destination, byte[] source, ulong number, byte[] plain, QuicKeys keys)
    {
        if (level is not (0 or 1) || destination.Length > 20 || source.Length > 20) throw new InvalidDataException("Invalid long header.");
        byte[] prefix = [(byte)(level == 0 ? 0xc3 : 0xe3), 0, 0, 0, 1, .. Wire.Vector(destination, 1), .. Wire.Vector(source, 1)];
        if (level == 0) prefix = [.. prefix, 0];
        // Initial datagrams are padded to 1200; their length field occupies two bytes.
        if (level == 0 && plain.Length < 1200 - prefix.Length - 22) Array.Resize(ref plain, 1200 - prefix.Length - 22);
        byte[] header = [.. prefix, .. QuicData.Int((ulong)plain.Length + 20), 0, 0, 0, 0];
        return keys.Seal(header, header.Length - 4, number, plain);
    }
    public static byte[] Short(byte[] destination, ulong number, byte[] plain, QuicKeys keys)
    {
        byte[] header = [0x43, .. destination, 0, 0, 0, 0];
        return keys.Seal(header, header.Length - 4, number, plain);
    }
    public static byte[] Versions(byte[] destination, byte[] source) => [0xc0, 0, 0, 0, 0, .. Wire.Vector(destination, 1), .. Wire.Vector(source, 1), 0, 0, 0, 1];
}
