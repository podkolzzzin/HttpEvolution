// 12_1997_HTTP1.1_CHUNKED: Передавання відповіді частинами: chunked.
using static DemoMode;
using static Http1Server;

return DemoCommand.Run(args, Http | Ssl3 | Ssl3Client, Http, options =>
{
    if (options.Ssl3Pin is { } ssl3Pin)
    {
        return DemoCommand.RunClient(() => LegacySsl.Client(ssl3Pin));
    }

    void Serve(Stream stream) => new Http11ChunkedConnection(stream).Run();

    if (options.Has(Ssl3))
    {
        Run(Serve, "HTTPS/SSL 3.0", Ssl3Port, stream => LegacySsl.TryAccept(stream), LegacyIdentity.Pin);
    }
    else
    {
        Run(Serve, "HTTP/1.1");
    }

    return 0;
});
