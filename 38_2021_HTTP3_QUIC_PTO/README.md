# 38 · 2021 · HTTP/3 · QUIC_PTO

QUIC ACK tracking and probe timeout.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The console demo runs this step, prints the result, and exits.

## Files added or changed in this step

- [Program.cs](Program.cs).
- [QuicRecovery.cs](../common/Quic/QuicRecovery.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u ../37_2021_HTTP3_QUIC_UDP/Program.cs Program.cs
diff -u /dev/null ../common/Quic/QuicRecovery.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
