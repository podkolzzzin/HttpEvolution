// 08_1996_HTTP1.0_SSL3_HTTPS: HTTPS with a custom SSL 3.0 handshake.
using static DemoMode;
using static Http1Server;

return DemoCommand.Run(args, Http | Ssl3, Ssl3, options =>
{
    void Serve(Stream stream) => new Http10HeadConnection(stream).Run();

    if (options.Has(Ssl3))
    {
        Run(Serve, "HTTPS/SSL 3.0", Ssl3Port, stream => LegacySsl.TryAccept(stream), LegacyIdentity.Pin);
    }
    else
    {
        Run(Serve, "HTTP/1.0");
    }

    return 0;
});
