# 40 · 2022 · HTTP/3 · QUIC_STREAMS

QUIC streams and flow control for HTTP/3.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The console demo runs this step, prints the result, and exits.

## Files added or changed in this step

- [Program.cs](Program.cs).
- [QuicSession.cs](QuicSession.cs).
- [QuicStreams.cs](../common/Quic/QuicStreams.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u ../39_2022_HTTP3_QPACK/Program.cs Program.cs
diff -u ../36_2021_HTTP3_QUIC_TLS/QuicSession.cs QuicSession.cs
diff -u /dev/null ../common/Quic/QuicStreams.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
