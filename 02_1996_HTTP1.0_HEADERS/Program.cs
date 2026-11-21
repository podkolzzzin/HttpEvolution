// 02_1996_HTTP1.0_HEADERS: Статус, заголовки та Content-Length.
Http1Server.Run(stream => new Http10Connection(stream).Run(), "HTTP/1.0");
