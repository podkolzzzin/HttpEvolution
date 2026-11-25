using System.Net;
using Http3Streams.Network;
using Http3Streams.Protocol;

namespace Http3Streams.Transport;

public sealed class Http3Client : IAsyncDisposable
{
    private readonly Http3Session session;
    private readonly string authority;
    private readonly Task inboundStreams;

    private Http3Client(Http3Session session, string authority)
    {
        this.session = session;
        this.authority = authority;
        inboundStreams = session.ReceiveAsync(null, CancellationToken.None);
    }

    public static async Task<Http3Client> ConnectAsync(
        IPEndPoint endpoint, string expectedFingerprint, CancellationToken cancellationToken = default)
    {
        var connection = await QuicConnection.ConnectAsync(endpoint, expectedFingerprint, cancellationToken);
        var session = await Http3Session.StartAsync(connection, cancellationToken);
        return new Http3Client(session, endpoint.ToString());
    }

    public async Task<(long StreamId, int Status, string Body)> GetAsync(
        string path, CancellationToken cancellationToken = default)
    {
        await using QuicStream stream = session.Connection.OpenStream(bidirectional: true);
        await Http3Message.WriteGetAsync(stream, authority, path, cancellationToken);
        stream.CompleteWrites();
        var response = await Http3Message.ReadResponseAsync(stream, cancellationToken);
        return (stream.Id, response.Status, response.Body);
    }

    public async ValueTask DisposeAsync()
    {
        await session.DisposeAsync();
        await inboundStreams;
    }
}
