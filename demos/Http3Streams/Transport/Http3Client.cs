using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography;
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
        var options = new QuicClientConnectionOptions
        {
            RemoteEndPoint = endpoint,
            MaxInboundBidirectionalStreams = 0,
            MaxInboundUnidirectionalStreams = 8,
            DefaultCloseErrorCode = Http3Session.NoError,
            DefaultStreamErrorCode = Http3Session.RequestCancelled,
            IdleTimeout = TimeSpan.FromSeconds(30),
            ClientAuthenticationOptions = new SslClientAuthenticationOptions
            {
                TargetHost = "localhost",
                ApplicationProtocols = [SslApplicationProtocol.Http3],
                RemoteCertificateValidationCallback = (_, certificate, _, _) =>
                    certificate is not null && string.Equals(
                        certificate.GetCertHashString(HashAlgorithmName.SHA256),
                        expectedFingerprint, StringComparison.OrdinalIgnoreCase)
            }
        };
        var connection = await QuicConnection.ConnectAsync(options, cancellationToken);
        var session = await Http3Session.StartAsync(connection, cancellationToken);
        return new Http3Client(session, endpoint.ToString());
    }

    public async Task<(long StreamId, int Status, string Body)> GetAsync(
        string path, CancellationToken cancellationToken = default)
    {
        await using QuicStream stream = await session.Connection.OpenOutboundStreamAsync(
            QuicStreamType.Bidirectional, cancellationToken);
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
