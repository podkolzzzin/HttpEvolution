using System.Security.Cryptography;

sealed class TlsStream(TlsRecords records) : Stream
{
    byte[] pending = [];
    int offset;
    bool closed, disposed;
    public static TlsStream? TryAccept(Stream transport, string protocol)
    {
        var records = new TlsRecords(transport);
        try
        {
            if (transport.CanTimeout) transport.ReadTimeout = transport.WriteTimeout = 5000;
            using var handshake = new TlsHandshake(protocol);
            do { records.Write(22, handshake.AcceptHello(records.Handshake())); }
            while (handshake.HandshakeSecret.Length == 0);
            records.Keys(handshake.ClientHandshakeTraffic, handshake.ServerHandshakeTraffic);
            foreach (var message in handshake.ServerFlight()) records.Write(22, message);
            handshake.VerifyFinished(records.Handshake());
            records.Compatibility = false;
            records.Keys(handshake.ClientTraffic, handshake.ServerTraffic);
            return new TlsStream(records);
        }
        catch (Exception error) when (error is IOException or InvalidDataException or CryptographicException)
        {
            try { records.Write(21, [2, 40]); } catch (IOException) { }
            records.Dispose(); return null;
        }
    }
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public override int Read(Span<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (buffer.Length == 0 || closed) return 0;
        while (offset == pending.Length)
        {
            var (type, data) = records.Read();
            if (type == 21 && data.Length == 2 && data[1] == 0) { closed = true; return 0; }
            if (type != 23) throw new IOException("Expected TLS application data.");
            pending = data; offset = 0;
        }
        var count = Math.Min(buffer.Length, pending.Length - offset);
        pending.AsSpan(offset, count).CopyTo(buffer); offset += count;
        return count;
    }
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        while (buffer.Length > 0)
        {
            var count = Math.Min(buffer.Length, 16384);
            records.Write(23, buffer[..count].ToArray()); buffer = buffer[count..];
        }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            try { records.Write(21, [1, 0]); } catch (IOException) { }
            disposed = true; records.Dispose();
        }
        base.Dispose(disposing);
    }
    public override bool CanRead => !disposed;
    public override bool CanWrite => !disposed;
    public override bool CanSeek => false;
    public override bool CanTimeout => records.Transport.CanTimeout;
    public override int ReadTimeout { get => records.Transport.ReadTimeout; set => records.Transport.ReadTimeout = value; }
    public override int WriteTimeout { get => records.Transport.WriteTimeout; set => records.Transport.WriteTimeout = value; }
    public override void Flush() => records.Transport.Flush();
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
