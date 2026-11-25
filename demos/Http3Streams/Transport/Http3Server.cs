using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Http3Streams.Application;
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

    public static async Task<Http3Server> ListenAsync(
        IPEndPoint endpoint, X509Certificate2 certificate, IRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        var options = new QuicServerConnectionOptions
        {
            MaxInboundBidirectionalStreams = 16,
            MaxInboundUnidirectionalStreams = 8,
            DefaultCloseErrorCode = Http3Session.NoError,
            DefaultStreamErrorCode = Http3Session.RequestCancelled,
            IdleTimeout = TimeSpan.FromSeconds(30),
            ServerAuthenticationOptions = new SslServerAuthenticationOptions
            {
                ApplicationProtocols = [SslApplicationProtocol.Http3],
                ServerCertificate = certificate
            }
        };
        var listener = await QuicListener.ListenAsync(new QuicListenerOptions
        {
            ListenEndPoint = endpoint,
            ApplicationProtocols = [SslApplicationProtocol.Http3],
            ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(options)
        }, cancellationToken);
        return new Http3Server(listener, handler);
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
                    await session.ReceiveAsync(ServeRequestAsync, token);
                }
                catch (QuicException)
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
            QuicConnection connection;
            try
            {
                connection = await listener.AcceptConnectionAsync(cancellationToken);
            }
            catch (AuthenticationException)
            {
                continue;
            }
            catch (QuicException error) when (error.QuicError is QuicError.ConnectionAborted or QuicError.ConnectionTimeout)
            {
                continue;
            }
            yield return connection;
        }
    }

    private async Task ServeRequestAsync(QuicStream stream, CancellationToken cancellationToken)
    {
        try
        {
            string path = await Http3Message.ReadGetAsync(stream, cancellationToken);
            var response = await handler.HandleAsync(path, cancellationToken);
            await Http3Message.WriteResponseAsync(stream, response.Status, response.Body, cancellationToken);
            stream.CompleteWrites();
        }
        catch (Exception error) when (error is InvalidDataException or EndOfStreamException)
        {
            stream.Abort(QuicAbortDirection.Both, 0x10e); // H3_MESSAGE_ERROR
        }
        catch (QuicException error) when (error.QuicError == QuicError.StreamAborted)
        {
            // The peer cancelled this request. Other streams can still complete.
        }
    }

    public ValueTask DisposeAsync() => listener.DisposeAsync();
}
