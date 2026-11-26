# 10 · 1997 · HTTP/1.1 · HOST

The required Host header in HTTP/1.1.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal.

```sh
curl --http1.1 -i http://127.0.0.1:8080/
```

## Files added or changed in this step

- [Http11HostConnection.cs](Http11HostConnection.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http11HostConnection.cs
diff -u ../09_1996_HTTP1.0_SSL3_CLIENT/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
