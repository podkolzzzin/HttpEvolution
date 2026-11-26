# 08 · 1996 · HTTP/1.0 · SSL3_HTTPS

HTTPS with a custom SSL 3.0 handshake.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

## Files added or changed in this step

- [LegacySsl.cs](LegacySsl.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null LegacySsl.cs
diff -u ../04_1996_HTTP1.0_SSL3_PRIMITIVES/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
