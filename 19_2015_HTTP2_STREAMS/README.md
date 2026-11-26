# 19 · 2015 · HTTP/2 · STREAMS

HTTP/2 GET and HEAD responses on streams.

Step 16 handles control frames. This step adds per-connection HPACK state and responses to GET and HEAD.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal.

```sh
curl --http2-prior-knowledge -i http://127.0.0.1:8081/
```

## Files added or changed in this step

- [Http2StreamConnection.cs](Http2StreamConnection.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http2StreamConnection.cs
diff -u ../17_2015_HTTP2_HPACK/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
