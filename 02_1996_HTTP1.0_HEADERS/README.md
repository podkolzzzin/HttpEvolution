# 02 · 1996 · HTTP/1.0 · HEADERS

Status, headers, and Content-Length.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal.

```sh
curl --http1.0 -i http://127.0.0.1:8080/
```

## Files added or changed in this step

- [Http10Connection.cs](Http10Connection.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http10Connection.cs
diff -u ../01_1991_HTTP0.9_GET/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
