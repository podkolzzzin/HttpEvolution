// 21_2015_HTTP2_CONTINUATION: HTTP/2: збирання блоків CONTINUATION.
Http2Server.Run(stream => new Http2ContinuationConnection(stream));
