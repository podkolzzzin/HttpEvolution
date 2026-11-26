# 03 · 1996 · HTTP/1.0 · HEAD

HEAD returns response headers without a body.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal.

```sh
curl --http1.0 -I http://127.0.0.1:8080/
```

## Files added or changed in this step

- [Http10HeadConnection.cs](Http10HeadConnection.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http10HeadConnection.cs
diff -u ../02_1996_HTTP1.0_HEADERS/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
