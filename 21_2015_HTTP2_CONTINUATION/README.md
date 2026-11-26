# 21 · 2015 · HTTP/2 · CONTINUATION

Assembly of HTTP/2 header blocks with CONTINUATION frames.

This step builds on step 20 by assembling HEADERS and CONTINUATION frames from the same stream before HPACK decoding.

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

- [Http2ContinuationConnection.cs](Http2ContinuationConnection.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http2ContinuationConnection.cs
diff -u ../20_2015_HTTP2_FLOW_CONTROL/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
