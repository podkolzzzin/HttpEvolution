# 13 · 1999 · HTTP/1.1 · TLS10

TLS 1.0 introduces a PRF, HMAC, and a new Finished message.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

Run the client in a second terminal with the pin printed by the server.

```sh
dotnet run -- --tls10-client PIN
```

## Files added or changed in this step

- [LegacyCipher.cs](LegacyCipher.cs).
- [LegacyRecords.cs](LegacyRecords.cs).
- [LegacySsl.Client.cs](LegacySsl.Client.cs).
- [LegacySsl.cs](LegacySsl.cs).
- [Program.cs](Program.cs).
- [Tls10Crypto.cs](../common/Tls/Tls10Crypto.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u ../04_1996_HTTP1.0_SSL3_PRIMITIVES/LegacyCipher.cs LegacyCipher.cs
diff -u ../04_1996_HTTP1.0_SSL3_PRIMITIVES/LegacyRecords.cs LegacyRecords.cs
diff -u ../09_1996_HTTP1.0_SSL3_CLIENT/LegacySsl.Client.cs LegacySsl.Client.cs
diff -u ../08_1996_HTTP1.0_SSL3_HTTPS/LegacySsl.cs LegacySsl.cs
diff -u ../12_1997_HTTP1.1_CHUNKED/Program.cs Program.cs
diff -u /dev/null ../common/Tls/Tls10Crypto.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
