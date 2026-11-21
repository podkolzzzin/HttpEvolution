// 19_2015_HTTP2_STREAMS: HTTP/2: відповіді GET/HEAD у потоках.
Http2Server.Run(stream => new Http2StreamConnection(stream));
