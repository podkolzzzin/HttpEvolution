# 02b · 1996 · HTTP/1.0 · Body compression

Start the server from the repository root and run the client commands in a second terminal.

```sh
dotnet run --project 02b_1996_HTTP1.0_CONTENT_ENCODING
```

## 1996 · Two versions of the same page

```sh
demo_dir=$(mktemp -d)
cd "$demo_dir"
url=http://127.0.0.1:8096

curl -sS --http1.0 -D - "$url/" -o page.html
curl -sS --http1.0 -D - -H 'Accept-Encoding: x-gzip' "$url/" -o page.gz
wc -c page.html page.gz
gzip -dc page.gz | cmp - page.html && echo 'The decompressed HTML matches'
```

The second response includes `Content-Encoding: x-gzip`. `Content-Length` gives the size of the gzip stream. Both responses have `Content-Type: text/html; charset=utf-8`. The server also accepts the modern name `gzip`.

In 1996, RFC 1945 described `x-gzip` and `x-compress` for HTTP/1.0. Its appendix lists `Accept-Encoding` among headers used by some implementations. [RFC 1945, §3.5](https://www.rfc-editor.org/rfc/rfc1945#section-3.5) · [Appendix D.2.3](https://www.rfc-editor.org/rfc/rfc1945#appendix-D.2.3).

## 2015 · Check whether the server accepts a compressed POST

```sh
awk 'BEGIN {for (i = 0; i < 64; i++) print "{\"event\":\"view\"}"}' > events.ndjson
gzip -nc events.ndjson > events.gz
wc -c events.ndjson events.gz

curl -si --http1.1 -H 'Content-Type: application/x-ndjson' -H 'Content-Encoding: gzip' \
  --data-binary @events.gz "$url/events"

curl -si --http1.1 -H 'Content-Type: application/x-ndjson' -H 'Content-Encoding: gzip' \
  --data-binary @events.gz "$url/legacy/events"

curl -si --http1.1 -H 'Content-Type: application/x-ndjson' \
  --data-binary @events.ndjson "$url/legacy/events"
```

1. `/events` decompresses gzip, prints both sizes on the server, and returns `204 No Content`.
2. `/legacy/events` accepts only uncompressed data. It rejects gzip with `415 Unsupported Media Type` and `Accept-Encoding: identity`.
3. Retry with the same events uncompressed to get `204`.

In November 2015, RFC 7694 described how a server uses `Accept-Encoding` in a **response** to advertise supported **request** content codings. Support depends on the resource. [RFC 7694, §3](https://www.rfc-editor.org/rfc/rfc7694#section-3).

The POST example reads a body with a known `Content-Length`. Both the transmitted and decompressed body are limited to 64 KiB. Press Ctrl+C to stop the server.

Compare representation selection with the previous step from this demo’s directory.

```sh
diff -u ../02_1996_HTTP1.0_HEADERS/Http10Connection.cs ContentCodingConnection.cs
```

[Source code](ContentCodingConnection.cs) · [All steps](../README.md).
