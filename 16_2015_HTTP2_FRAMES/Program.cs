// 16_2015_HTTP2_FRAMES: HTTP/2 connection preface, SETTINGS, and frames over TCP.
Http2Server.Run(stream => new Http2Connection(stream));
