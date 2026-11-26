# 20 · 2015 · HTTP/2 · FLOW_CONTROL

HTTP/2 flow-control windows.

This step adds connection and stream windows, a queue of response bodies, and WINDOW_UPDATE handling to step 19.

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

- [Http2FlowControlConnection.cs](Http2FlowControlConnection.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http2FlowControlConnection.cs
diff -u ../19_2015_HTTP2_STREAMS/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
