# 30 · 2021 · HTTP/3 · QUIC v1 primitives

Run [Program.cs](Program.cs) from this directory to see the full exchange in memory.

```sh
dotnet run
```

| Feature | Console output | Implementation |
| --- | --- | --- |
| Varint | Four integers encoded in 1, 2, 4, and 8 bytes and the decoded values | [QuicData](../common/Quic/QuicData.cs) |
| Initial packet | A 1,200-byte datagram with the version and both connection IDs | [QuicPacket](../common/Quic/QuicPacket.cs) |
| Packet protection | The client Initial keys remove the header mask and decrypt packet 7, which contains a PING frame | [QuicKeys](../common/Quic/QuicKeys.cs) |
| Stream offsets | The fragment with FIN arrives first. Once the start arrives, the stream can return `HelloQUIC!` | [QuicBuffer](../common/Quic/QuicBuffer.cs) |
| ACK ranges | One frame acknowledges packet numbers in three ranges | [QuicAcks](../common/Quic/QuicAcks.cs) |
| Transport parameters | The demo verifies the client SCID and reads the server’s limits on data, streams, and datagram size | [QuicParameters](../common/Quic/QuicParameters.cs) |

Compare with the previous step.

```sh
diff -u ../29_2018_HTTP2_ALPN/Program.cs Program.cs
```

[Next: the TLS handshake in QUIC](../36_2021_HTTP3_QUIC_TLS/). [All steps](../README.md).
