// 42_2022_HTTP3_QUIC_RECOVERY: QUIC loss recovery for HTTP/3 with new packet numbers on retransmission.
using static DemoMode;
using static Http1Server;

return await DemoCommand.RunAsync(args, Http | Ssl3 | Ssl3Client | Tls10 | Tls10Client | Tls13 | Http2 | Http3 | Sse | WebSocket, Http3, async options =>
{
    if (options.Ssl3Pin is { } ssl3Pin)
    {
        return DemoCommand.RunClient(() => LegacySsl.Client(ssl3Pin, 0));
    }

    if (options.Tls10Pin is { } tls10Pin)
    {
        return DemoCommand.RunClient(() => LegacySsl.Client(tls10Pin, 1));
    }

    if (options.Has(Sse) || options.Has(WebSocket))
    {
        RealtimeServer.Run(webSocket: options.Has(WebSocket));
        return 0;
    }

    if (options.Has(Http3))
    {
        await QuicServer.Run();
        return 0;
    }

    if (options.Has(Http2))
    {
        Http2Alpn.Run(options.Has(Tls13));
        return 0;
    }

    void Serve(Stream stream) => new Http11ChunkedConnection(stream).Run();

    if (options.Has(Ssl3))
    {
        Run(Serve, "HTTPS/SSL 3.0", Ssl3Port, stream => LegacySsl.TryAccept(stream, 0), LegacyIdentity.Pin);
    }
    else if (options.Has(Tls10))
    {
        Run(Serve, "HTTPS/TLS 1.0", Tls10Port, stream => LegacySsl.TryAccept(stream, 1), LegacyIdentity.Pin);
    }
    else if (options.Has(Tls13))
    {
        Run(Serve, "HTTPS/TLS 1.3", Tls13Port, stream => TlsStream.TryAccept(stream, "http/1.1"));
    }
    else
    {
        Run(Serve, "HTTP/1.1");
    }

    return 0;
});
