using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Http3Streams.Application;
using Http3Streams.Network;
using Http3Streams.Protocol;

namespace Http3Streams.Transport;

public sealed class Http3Server : IAsyncDisposable
{
    private readonly QuicListener listener;
    private readonly IRequestHandler handler;

    private Http3Server(QuicListener listener, IRequestHandler handler)
    {
        this.listener = listener;
        this.handler = handler;
    }

    public IPEndPoint LocalEndPoint => listener.LocalEndPoint;
    internal event Action<string, long, string, int>? RequestServed;

    public static Task<Http3Server> ListenAsync(
        IPEndPoint endpoint, X509Certificate2 certificate, IRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var listener = new QuicListener(endpoint, certificate);
        return Task.FromResult(new Http3Server(listener, handler));
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        return Parallel.ForEachAsync(AcceptConnectionsAsync(),
            new ParallelOptions { MaxDegreeOfParallelism = 16, CancellationToken = cancellationToken },
            async (connection, token) =>
            {
                try
                {
                    await using var session = await Http3Session.StartAsync(connection, token);
                    await session.ReceiveAsync((stream, ct) => ServeRequestAsync(connection, stream, ct), token);
                }
                catch (IOException)
                {
                    // A disconnected peer only ends its own session.
                }
            });
    }

    private async IAsyncEnumerable<QuicConnection> AcceptConnectionsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (true)
        {
            yield return await listener.AcceptAsync(cancellationToken);
        }
    }

    private async Task ServeRequestAsync(QuicConnection connection, QuicStream stream, CancellationToken cancellationToken)
    {
        try
        {
            string path = await Http3Message.ReadGetAsync(stream, cancellationToken);
            var response = await handler.HandleAsync(path, cancellationToken);
            await Http3Message.WriteResponseAsync(stream, response.Status, response.Body, cancellationToken);
            stream.CompleteWrites();
            RequestServed?.Invoke(connection.Id, stream.Id, path, response.Status);
        }
        catch (Exception error) when (error is InvalidDataException or EndOfStreamException)
        {
            stream.Abort(0x10e); // H3_MESSAGE_ERROR
        }
        catch (QuicStreamAbortedException)
        {
            // The peer cancelled this request. Other streams can still complete.
        }
    }

    public ValueTask DisposeAsync() => listener.DisposeAsync();
}
