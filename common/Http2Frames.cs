using System.Buffers.Binary;

static class Http2Frames
{
    public static (byte Type, byte Flags, int Id, byte[] Data) Read(Stream stream)
    {
        Span<byte> header = stackalloc byte[9];
        stream.ReadExactly(header);
        var length = (header[0] << 16) | (header[1] << 8) | header[2];
        if (length > 16384) throw new InvalidDataException("Frame exceeds the advertised maximum.");
        var data = new byte[length];
        stream.ReadExactly(data);
        return (header[3], header[4], BinaryPrimitives.ReadInt32BigEndian(header[5..]) & 0x7fffffff, data);
    }

    public static void Write(Stream stream, byte type, byte flags, int id, byte[] data)
    {
        Span<byte> header = stackalloc byte[9];
        header[0] = (byte)(data.Length >> 16); header[1] = (byte)(data.Length >> 8); header[2] = (byte)data.Length;
        header[3] = type; header[4] = flags;
        BinaryPrimitives.WriteInt32BigEndian(header[5..], id);
        stream.Write(header);
        stream.Write(data);
    }
}
