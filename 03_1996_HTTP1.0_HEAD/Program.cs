// 03_1996_HTTP1.0_HEAD: HEAD: заголовки без тіла відповіді.
Http1Server.Run(stream => new Http10HeadConnection(stream).Run(), "HTTP/1.0");
