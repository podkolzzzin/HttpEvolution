namespace Http3Streams.Network;

internal sealed class QuicStreamAbortedException(long id, long errorCode)
    : IOException($"QUIC stream {id} was reset (0x{errorCode:x}).");

// Reads expose contiguous bytes. Completing writes sends a FIN at the next byte offset.
internal sealed class QuicStream(long id, bool initiatedHere, QuicConnection connection) : Stream
{
    private readonly ReceiveBuffer input = new();
    private TaskCompletionSource? reader;
    private Exception? failure;
    private long written;
    private bool sentFin;
    private bool disposed;

    internal long Id { get; } = id;
    internal bool Bidirectional => (Id & 2) == 0;
    public override bool CanRead => !disposed && (Bidirectional || !initiatedHere);
    public override bool CanWrite => !disposed && (Bidirectional || initiatedHere) && !sentFin;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    // Called by the connection under its lock.
    internal void Receive(long offset, byte[] data, bool fin)
    {
        if (!Bidirectional && initiatedHere)
        {
            throw new InvalidDataException("The peer wrote to a send-only stream.");
        }
        if (failure is null)
        {
            input.Insert(offset, data, fin);
            WakeReader();
        }
    }

    internal void Fail(Exception error)
    {
        failure ??= error;
        WakeReader();
    }

    private void WakeReader()
    {
        reader?.TrySetResult();
        reader = null;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Task changed;
            lock (connection.Gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                if (!CanRead)
                {
                    throw new NotSupportedException("This QUIC stream is send-only.");
                }
                if (failure is not null)
                {
                    throw failure;
                }
                int count = input.Read(buffer.Span);
                if (count != 0 || buffer.IsEmpty || input.AtEnd)
                {
                    return count;
                }
                reader ??= new(TaskCreationOptions.RunContinuationsAsynchronously);
                changed = reader.Task;
            }
            await changed.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (connection.Gate)
        {
            EnsureWritable();
            connection.QueueData(Id, written, buffer, false);
            written += buffer.Length;
        }
        return ValueTask.CompletedTask;
    }

    internal void CompleteWrites()
    {
        lock (connection.Gate)
        {
            EnsureWritable();
            connection.QueueData(Id, written, ReadOnlyMemory<byte>.Empty, true);
            sentFin = true;
        }
    }

    private void EnsureWritable()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (failure is not null)
        {
            throw failure;
        }
        if (!CanWrite)
        {
            throw new InvalidOperationException("This QUIC stream has no open send side.");
        }
    }

    internal void Abort(long errorCode)
    {
        lock (connection.Gate)
        {
            if (failure is null && !disposed)
            {
                connection.AbortStream(Id, errorCode);
                Fail(new QuicStreamAbortedException(Id, errorCode));
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        lock (connection.Gate)
        {
            if (disposing && !disposed)
            {
                if (failure is null && (CanWrite || (CanRead && !input.AtEnd)))
                {
                    Abort(0x10c); // H3_REQUEST_CANCELLED
                }
                disposed = true;
                WakeReader();
            }
        }
        base.Dispose(disposing);
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();

    public override void Write(byte[] buffer, int offset, int count) =>
        WriteAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token) =>
        ReadAsync(buffer.AsMemory(offset, count), token).AsTask();

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token) =>
        WriteAsync(buffer.AsMemory(offset, count), token).AsTask();

    public override void Flush()
    {
    }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
