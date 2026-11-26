# 01 · 1991 · HTTP/0.9 · GET

GET → HTML → TCP close.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal.

```sh
printf 'GET /\r\n' | nc 127.0.0.1 8080
```

## Files added or changed in this step

- [Http09Connection.cs](Http09Connection.cs).
- [Program.cs](Program.cs).

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
