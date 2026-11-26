# 16 · 2015 · HTTP/2 · FRAMES

The HTTP/2 connection preface, SETTINGS, and frames over TCP.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

This step handles SETTINGS and PING. Step 19 adds GET and HEAD.

Send the preface and an empty SETTINGS frame in a second terminal to see the reply in hex.

```sh
printf 'PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n\000\000\000\004\000\000\000\000\000' | nc -N 127.0.0.1 8081 | od -An -tx1
```

## Files added or changed in this step

- [Http2Server.cs](Http2Server.cs).
- [Http2Frames.cs](../common/Http2/Http2Frames.cs).
- [Http2Connection.cs](Http2Connection.cs).
- [Program.cs](Program.cs).

Compare with the HTTP/1.1 server in step 15. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http2Connection.cs
diff -u /dev/null Http2Server.cs
diff -u /dev/null ../common/Http2/Http2Frames.cs
diff -u ../15_2010_HTTP1.1_TLS_RENEGOTIATION/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
