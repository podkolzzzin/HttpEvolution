// 03_1996_HTTP1.0_HEAD: HEAD returns response headers without a body.
Http1Server.Run(stream => new Http10HeadConnection(stream).Run(), "HTTP/1.0");
