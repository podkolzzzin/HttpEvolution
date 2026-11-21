using System.Security.Cryptography;

internal sealed class LegacyRecords(Stream stream, byte minor = 0) : IRecordLayer
{
    private LegacyCipher? read;
    private LegacyCipher? write;
    private LegacyCipher? nextRead;
    private LegacyCipher? nextWrite;
    private byte[] pending = [];
    public Stream Transport => stream;

    public void Keys(byte[] block, bool server)
    {
        if (read is not null || write is not null || nextRead is not null || nextWrite is not null)
        {
            throw new IOException("SSL renegotiation is outside this demo.");
        }

        var clientKeys = new LegacyCipher(block[..20], block[40..64], block[88..96], minor);
        var serverKeys = new LegacyCipher(block[20..40], block[64..88], block[96..104], minor);
        nextRead = server ? clientKeys : serverKeys;
        nextWrite = server ? serverKeys : clientKeys;
    }

    public void ReadChange()
    {
        var (type, data) = Read();
        if (type != 20 || !data.AsSpan().SequenceEqual(new byte[] { 1 }) || pending.Length != 0 || nextRead is null)
        {
            throw new IOException("Expected ChangeCipherSpec.");
        }

        read = nextRead;
        nextRead = null;
    }

    public void WriteChange()
    {
        if (nextWrite is null)
        {
            throw new IOException("Missing pending cipher.");
        }

        Write(20, [1]);
        write = nextWrite;
        nextWrite = null;
    }

    public (byte Type, byte[] Data) Read()
    {
        var header = new byte[5];
        stream.ReadExactly(header);
        var length = (header[3] << 8) | header[4];
        if (header[1] != 3 || header[2] != minor || header[0] is < 20 or > 23 || length > (read is null ? 16384 : 18432))
        {
            throw new IOException("Invalid SSL/TLS record.");
        }

        var data = new byte[length];
        stream.ReadExactly(data);
        try
        {
            if (read is not null)
            {
                data = read.Open(header[0], data);
            }
        }
        catch (CryptographicException error)
        {
            throw new IOException("Invalid SSL record.", error);
        }

        if (data.Length > 16384)
        {
            throw new IOException("SSL plaintext too large.");
        }

        return (header[0], data);
    }

    public void Write(byte type, byte[] data)
    {
        if (data.Length > 16384)
        {
            throw new IOException("SSL plaintext too large.");
        }

        if (write is not null)
        {
            data = write.Seal(type, data);
        }

        stream.Write(new byte[] { type, 3, minor, (byte)(data.Length >> 8), (byte)data.Length });
        stream.Write(data);
    }

    public byte[] Handshake() => RecordMessages.Read(this, ref pending);
    public void Dispose()
    {
        read?.Dispose();
        write?.Dispose();
        nextRead?.Dispose();
        nextWrite?.Dispose();
        stream.Dispose();
    }
}
