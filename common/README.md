# Shared code

Each demo links to the shared source files it uses with `Compile Include` and `Link`.

```xml
<Compile Include="../common/Wire/Wire.cs" Link="common/Wire/Wire.cs" />
```

`Include` points to the source file. `Link` places it in the `common` subtree in the IDE. The files compile as part of each demo project.

| Directory | Contents |
| --- | --- |
| [Demo](Demo/) | Command-line arguments with System.CommandLine and a ClientHello helper for console demos |
| [HeaderCompression](HeaderCompression/) | HPACK, QPACK, Huffman coding, and shared header formats |
| [Http1](Http1/) | TCP accept loop for HTTP/0.9 and HTTP/1.x |
| [Http2](Http2/) | Frame types, reading, and writing |
| [Quic](Quic/) | Packets, keys, ACKs, transport parameters, streams, and recovery |
| [Tls](Tls/) | SSL and TLS records, encryption, identity, and streams |
| [Wire](Wire/) | Integer and byte-vector handling for SSL, TLS, and QUIC |

Each step has its own `Program.cs`. Classes that introduce new behavior live in the corresponding step. For example, HTTP/2 starts with [frames and connection setup](../16_2015_HTTP2_FRAMES/Http2Connection.cs), then adds [responses on streams](../19_2015_HTTP2_STREAMS/Http2StreamConnection.cs), [flow-control windows](../20_2015_HTTP2_FLOW_CONTROL/Http2FlowControlConnection.cs), [CONTINUATION](../21_2015_HTTP2_CONTINUATION/Http2ContinuationConnection.cs), and [TLS with ALPN](../29_2018_HTTP2_ALPN/Http2Alpn.cs).

The [TLS 1.3 handshake in step 28](../28_2018_HTTP2_TLS13_HTTPS/TlsStream.Tls13.cs) and [SPKI pinning in step 37](../37_2021_HTTP3_QUIC_UDP/TlsCertificate.Pin.cs) extend the shared types through partial classes.

[All steps and run commands](../README.md).
