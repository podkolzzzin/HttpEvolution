// 09_1996_HTTP1.0_SSL3_CLIENT: SSL 3.0 with SPKI pin verification in TcpClient.
using static DemoMode;
using static Http1Server;

return DemoCommand.Run(args, Http | Ssl3 | Ssl3Client, Ssl3, options =>
{
    if (options.Ssl3Pin is { } ssl3Pin)
    {
        return DemoCommand.RunClient(() => LegacySsl.Client(ssl3Pin));
    }

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
