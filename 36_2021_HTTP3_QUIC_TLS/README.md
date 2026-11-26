# 36 · 2021 · HTTP/3 · QUIC_TLS

The TLS handshake across QUIC encryption levels.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The console demo runs this step, prints the result, and exits.

The demo creates a protected QUIC Initial packet containing a ClientHello and generates the server response in memory. Step 37 adds the UDP server.

## Files added or changed in this step

- [DemoClientHello.cs](../common/Demo/DemoClientHello.cs).
- [Program.cs](Program.cs).
- [QuicSession.cs](QuicSession.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u /dev/null ../common/Demo/DemoClientHello.cs
diff -u ../30_2021_HTTP3_QUIC_PRIMITIVES/Program.cs Program.cs
diff -u /dev/null QuicSession.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
