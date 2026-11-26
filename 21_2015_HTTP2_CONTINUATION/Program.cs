// 21_2015_HTTP2_CONTINUATION: HTTP/2 header block assembly with CONTINUATION frames.
Http2Server.Run(stream => new Http2ContinuationConnection(stream));
