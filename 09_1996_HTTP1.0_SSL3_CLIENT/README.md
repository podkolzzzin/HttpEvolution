# 09 · 1996 · HTTP/1.0 · SSL3_CLIENT

SSL 3.0 with SPKI pin verification in `TcpClient`.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal with the pin printed by the server.

```sh
dotnet run -- --ssl3-client PIN
```

## Files added or changed in this step

- [LegacySsl.Client.cs](LegacySsl.Client.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null LegacySsl.Client.cs
diff -u ../08_1996_HTTP1.0_SSL3_HTTPS/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
