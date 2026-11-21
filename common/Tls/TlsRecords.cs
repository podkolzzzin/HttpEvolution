using System.Security.Cryptography;

internal sealed class TlsRecords(Stream stream) : IRecordLayer
{
    private TlsCipher? read;
    private TlsCipher? write;
    private ulong readSequence;
    private ulong writeSequence;
    private byte[] handshake = [];
    public Stream Transport => stream;
    public bool Compatibility
    {
        get;
        set;
    } = true;

    public void Keys(byte[] client, byte[] server)
    {
        if (handshake.Length != 0)
        {
            throw new IOException("Handshake crossed a key boundary.");
        }

        read?.Dispose();
        write?.Dispose();
        read = new TlsCipher(client);
        write = new TlsCipher(server);
        readSequence = writeSequence = 0;
    }

    public TlsRecord Read()
    {
        while (true)
        {
            var header = new byte[5];
            stream.ReadExactly(header);
            var length = (header[3] << 8) | header[4];
            if (header[1] != 3 || length > (read is null ? 16384 : 16640))
            {
                throw new IOException("Invalid TLS record.");
            }

            var data = new byte[length];
            stream.ReadExactly(data);
            if (header[0] == 20 && Compatibility && data.AsSpan().SequenceEqual(new byte[] { 1 }))
            {
                continue;
            }

            if (read is null)
            {
                if (header[0] is not (21 or 22))
                {
                    throw new IOException("Expected plaintext handshake or alert.");
                }

                return new(header[0], data);
            }

            if (header[0] != 23 || header[2] != 3 || readSequence >= (1UL << 24))
            {
                throw new IOException("Invalid encrypted TLS record.");
            }

            try
            {
                data = read.Open(readSequence++, data, header);
            }
            catch (Exception error) when (error is CryptographicException or InvalidDataException)
            {
                throw new IOException("TLS authentication failed.", error);
            }

            var end = data.Length - 1;
            while (end >= 0 && data[end] == 0)
            {
                end--;
            }

            if (end < 0 || data[end] is not (21 or 22 or 23) || end > 16384)
            {
                throw new IOException("Invalid TLS inner plaintext.");
            }

            return new(data[end], data[..end]);
        }
    }

    public void Write(byte type, byte[] data)
    {
        if (data.Length > 16384 || writeSequence >= (1UL << 24))
        {
            throw new IOException("TLS record limit.");
        }

        byte[] header = [write is null ? type : (byte)23, 3, 3, .. Wire.Size(data.Length + (write is null ? 0 : 17))];
        if (write is not null)
        {
            data = write.Seal(writeSequence++, [.. data, type], header);
        }

        stream.Write(header);
        stream.Write(data);
    }

    public byte[] Handshake() => RecordMessages.Read(this, ref handshake);
    public void Dispose()
    {
        read?.Dispose();
        write?.Dispose();
        stream.Dispose();
    }
}
