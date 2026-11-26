// 14_2002_HTTP1.1_TLS_AES: AES-CBC support in TLS 1.0.
using static DemoMode;
using static Http1Server;

return DemoCommand.Run(args, Http | Ssl3 | Ssl3Client | Tls10 | Tls10Client, Tls10, options =>
{
    if (options.Ssl3Pin is { } ssl3Pin)
    {
        return DemoCommand.RunClient(() => LegacySsl.Client(ssl3Pin, 0));
    }

    if (options.Tls10Pin is { } tls10Pin)
    {
        return DemoCommand.RunClient(() => LegacySsl.Client(tls10Pin, 1));
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
    else
    {
        Run(Serve, "HTTP/1.1");
    }

    return 0;
});
