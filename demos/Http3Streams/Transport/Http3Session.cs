using System.Net.Quic;
using System.Runtime.CompilerServices;
using Http3Streams.Protocol;

namespace Http3Streams.Transport;

// Each peer owns one control stream for the entire lifetime of the connection.
internal sealed class Http3Session : IAsyncDisposable
{
    internal const long NoError = 0x100;
    internal const long RequestCancelled = 0x10c;
    private readonly QuicStream control;
    private int peerControlOpened;

    private Http3Session(QuicConnection connection, QuicStream control)
    {
        Connection = connection;
        this.control = control;
    }

    internal QuicConnection Connection
    {
        get;
    }

    internal static async Task<Http3Session> StartAsync(QuicConnection connection, CancellationToken cancellationToken)
    {
        QuicStream? control = null;
        try
        {
            control = await connection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional, cancellationToken);
            // Stream type 0 (control), frame type 4 (SETTINGS), empty payload (static QPACK only).
            await control.WriteAsync(new byte[] { 0, 4, 0 }, cancellationToken);
            return new Http3Session(connection, control);
        }
        catch
        {
            await connection.DisposeAsync();
            if (control is not null)
            {
                await control.DisposeAsync();
            }
            throw;
        }
    }

    internal async Task ReceiveAsync(
        Func<QuicStream, CancellationToken, Task>? onRequest, CancellationToken cancellationToken)
    {
        try
        {
            // Long-lived control streams must not occupy the last worker available to requests.
            await Parallel.ForEachAsync(AcceptStreamsAsync(),
                new ParallelOptions { MaxDegreeOfParallelism = 32, CancellationToken = cancellationToken },
                async (stream, token) =>
                {
                    await using (stream)
                    {
                        if (stream.Type == QuicStreamType.Bidirectional && onRequest is not null)
                        {
                            await onRequest(stream, token);
                        }
                        else
                        {
                            await ReadPeerStreamAsync(stream, token);
                        }
                    }
                });
        }
        catch (QuicException error) when (error.QuicError is QuicError.ConnectionAborted
            or QuicError.OperationAborted or QuicError.ConnectionIdle or QuicError.ConnectionTimeout)
        {
            // Local or remote shutdown ends the accept loop and its stream workers together.
        }
        catch (Exception error) when (error is InvalidDataException or EndOfStreamException)
        {
            await Connection.CloseAsync(0x101, CancellationToken.None); // H3_GENERAL_PROTOCOL_ERROR
        }
    }

    private async IAsyncEnumerable<QuicStream> AcceptStreamsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (true)
        {
            yield return await Connection.AcceptInboundStreamAsync(cancellationToken);
        }
    }

    private async Task ReadPeerStreamAsync(QuicStream stream, CancellationToken cancellationToken)
    {
        long type = await QuicInteger.ReadAsync(stream, cancellationToken);
        if (type == 0)
        {
            if (Interlocked.Exchange(ref peerControlOpened, 1) != 0
                || await QuicInteger.ReadAsync(stream, cancellationToken) != 4)
            {
                throw new InvalidDataException("Expected one HTTP/3 control stream beginning with SETTINGS.");
            }
            long length = await QuicInteger.ReadAsync(stream, cancellationToken);
            if (length > 4096)
            {
                throw new InvalidDataException("Peer SETTINGS exceed the demo's 4 KiB limit.");
            }
            var payload = new byte[(int)length];
            await stream.ReadExactlyAsync(payload, cancellationToken);
            using var settings = new MemoryStream(payload);
            var ids = new HashSet<long>();
            while (settings.Position < settings.Length)
            {
                long id = await QuicInteger.ReadAsync(settings, cancellationToken);
                if (!ids.Add(id) || id is 2 or 3 or 4 or 5)
                {
                    throw new InvalidDataException("Duplicate or reserved HTTP/3 setting.");
                }
                // We use the static table even when the peer advertises dynamic capacity.
                await QuicInteger.ReadAsync(settings, cancellationToken);
            }
        }

        // The demo emits no dynamic QPACK instructions. Drain peer extension streams as they arrive.
        await stream.CopyToAsync(Stream.Null, cancellationToken);
        if (type is 0 or 2 or 3)
        {
            throw new InvalidDataException("The peer closed a critical HTTP/3 stream.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await Connection.CloseAsync(NoError, CancellationToken.None);
        }
        finally
        {
            await control.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
