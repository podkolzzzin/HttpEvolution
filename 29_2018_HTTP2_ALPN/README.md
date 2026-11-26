# 29 · 2018 · HTTP/2 · ALPN

HTTP/2 over TLS 1.3 with ALPN `h2`.

This step uses the connection from step 21. TLS 1.3 negotiates `h2` through ALPN before the server handles frames.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal.

```sh
curl -k --http2 -i https://localhost:8444/
```

## Files added or changed in this step

- [Http2Alpn.cs](Http2Alpn.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http2Alpn.cs
diff -u ../28_2018_HTTP2_TLS13_HTTPS/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
