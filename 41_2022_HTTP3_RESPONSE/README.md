# 41 · 2022 · HTTP/3 · RESPONSE

HTTP/3 GET and HEAD over QUIC, with HEADERS, DATA, and FIN.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

See the [main README](../README.md#http3) for the HTTP/3 client and SPKI pin.

## Files added or changed in this step

- [Http3Connection.cs](Http3Connection.cs).
- [Program.cs](Program.cs).
- [QuicServer.cs](QuicServer.cs).
- [QuicSession.cs](QuicSession.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null Http3Connection.cs
diff -u ../40_2022_HTTP3_QUIC_STREAMS/Program.cs Program.cs
diff -u ../37_2021_HTTP3_QUIC_UDP/QuicServer.cs QuicServer.cs
diff -u ../40_2022_HTTP3_QUIC_STREAMS/QuicSession.cs QuicSession.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
