# 42 · 2022 · HTTP/3 · QUIC_RECOVERY

QUIC loss recovery for HTTP/3 with new packet numbers on retransmission.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

The demo starts a local server. Press Ctrl+C to stop it.

See the [main README](../README.md#http3) for the HTTP/3 client and SPKI pin.

The default is HTTP/3 with loss recovery. Other modes include `--http`, [`--sse` and `--ws`](../15b_2011_HTTP1.1_REALTIME/), `--ssl3`, `--tls10`, `--tls`, `--h2`, `--h2 --tls`, and clients with certificate pinning.

## Files added or changed in this step

- [Program.cs](Program.cs).
- [QuicSession.cs](QuicSession.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u ../41_2022_HTTP3_RESPONSE/Program.cs Program.cs
diff -u ../41_2022_HTTP3_RESPONSE/QuicSession.cs QuicSession.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
