internal static class Http2Alpn
{
    private const int TlsPort = 8444;
    private const string Protocol = "h2";

    public static void Run(bool tls)
    {
        if (tls)
        {
            Http2Server.Run(stream => new Http2ContinuationConnection(stream),
                TlsPort, "HTTP/2+TLS 1.3", stream => TlsStream.TryAccept(stream, Protocol));
        }
        else
        {
            Http2Server.Run(stream => new Http2ContinuationConnection(stream));
        }
    }
}
