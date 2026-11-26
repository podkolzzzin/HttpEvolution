# 15 · 2010 · HTTP/1.1 · TLS_RENEGOTIATION

TLS: renegotiation indication.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal with the pin printed by the server.

```sh
dotnet run -- --tls10-client PIN
```

Run the AES client in a second terminal.

```sh
printf 'GET / HTTP/1.0\r\n\r\n' | openssl s_client -quiet -tls1 -cipher 'AES128-SHA:@SECLEVEL=0' -connect 127.0.0.1:8441
```

## Files added or changed in this step

- [LegacySsl.cs](LegacySsl.cs).
- [Program.cs](Program.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u ../14_2002_HTTP1.1_TLS_AES/LegacySsl.cs LegacySsl.cs
diff -u ../14_2002_HTTP1.1_TLS_AES/Program.cs Program.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
