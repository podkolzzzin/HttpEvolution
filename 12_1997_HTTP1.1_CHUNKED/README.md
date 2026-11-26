# 12 · 1997 · HTTP/1.1 · CHUNKED

Chunked response transfer.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal.

```sh
curl --http1.1 --raw -i http://127.0.0.1:8080/chunks
```

## Files added or changed in this step

- [Http11ChunkedConnection.cs](Http11ChunkedConnection.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http11ChunkedConnection.cs
diff -u ../11_1997_HTTP1.1_KEEP_ALIVE/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
