# 37 · 2021 · HTTP/3 · QUIC_UDP

Accepting QUIC sessions with `UdpClient`.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

The UDP server demonstrates the QUIC/TLS handshake. Step 41 adds a full HTTP/3 response.

## Files added or changed in this step

- [Program.cs](Program.cs).
- [QuicServer.cs](QuicServer.cs).
- [TlsCertificate.Pin.cs](TlsCertificate.Pin.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u ../36_2021_HTTP3_QUIC_TLS/Program.cs Program.cs
diff -u /dev/null QuicServer.cs
diff -u /dev/null TlsCertificate.Pin.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
