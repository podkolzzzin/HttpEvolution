// 02_1996_HTTP1.0_HEADERS: Status, headers, and Content-Length.
Http1Server.Run(stream => new Http10Connection(stream).Run(), "HTTP/1.0");
