# 28 · 2018 · HTTP/2 · TLS13_HTTPS

HTTPS over a custom TLS 1.3 stream.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal.

```sh
curl -k --http1.1 -i https://localhost:8443/
```

## Files added or changed in this step

- [Program.cs](Program.cs).
- [TlsStream.Tls13.cs](TlsStream.Tls13.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u ../25_2018_HTTP2_TLS13_AUTH_RECORDS/Program.cs Program.cs
diff -u /dev/null TlsStream.Tls13.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
