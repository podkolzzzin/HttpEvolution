# HttpEvolution

Follow HTTP from 0.9 to HTTP/3 through **31 runnable C# demos**. Each project adds a protocol feature, from a single GET over TCP to HTTP/3 over QUIC.

[Open the interactive presentation](https://slides.podkolzin.consulting/http-evolution/)

Each directory starts with the step number and includes a year and a topic. Some numbers are skipped because related examples were combined. Open `HttpEvolution.slnx` to browse all steps, or open `HttpEvolution.csproj` in the directory of a particular step.

The demos use `TcpListener`, `TcpClient`, `UdpClient`, and byte streams. The project implements TLS, QUIC, HPACK, and QPACK directly and uses the .NET base class library for cryptographic primitives. `System.CommandLine` handles the server command-line arguments. Build with the **.NET 10 SDK**. The servers listen on loopback and are designed for local learning, including demonstrations of legacy SSL and TLS.

The [`.editorconfig`](.editorconfig) specifies four-space indentation, braces on separate lines, blocks for conditionals and loops, and separate declarations and statements. Check formatting with `dotnet format HttpEvolution.slnx --verify-no-changes --no-restore`.

## Presentation

The [front end](presentation/) uses React, TypeScript, and Vite. It requires Node.js 20.19 or later in the 20.x series, or Node.js 22.12 or later. Run these commands from the repository root.

```sh
cd presentation
npm ci
npm run dev
```

`npm run build` checks types and writes a static site to `presentation/dist/`. `npm run preview` serves the build locally. Slide URLs use fragments, so the build can be hosted at a subpath such as `/http-evolution/`.

The 68 slides include code, runnable commands, and links to the corresponding steps. On a phone, navigate with swipes, buttons, or the table of contents.

- **Body compression.** Open `#http10-gzip` for response sizes, `#request-compression` for a compressed POST and retry after a 415, or `#brotli-zstd` to compare codecs. The [local compression server](02b_1996_HTTP1.0_CONTENT_ENCODING/) reproduces the GET and POST examples with curl.
- **Real-time delivery.** SSE shows events in an HTTP response and an EventSource client. The WebSocket slides cover the upgrade, frames, and two-way messaging. The SSE chat animation uses POST requests alongside open `text/event-stream` responses. The WebSocket animation shows the same conversation over two-way channels. The WebRTC demo exchanges test video and text between two peers in one tab. Run the [SSE and WebSocket servers](15b_2011_HTTP1.1_REALTIME/) locally to see the exchanges in the browser.
- **SSL and host privacy.** The SSL slides cover Netscape's needs, the unreleased SSL 1.0, the limitations of SSL 2.0, and the decision to start the demo at SSL 3.0. They show certificate fields, SPKI pinning, RSA key exchange, and record keys. An interactive record diagram compares what is visible on the wire with the HTTP data, MAC, and padding recovered on the server. The Host slides model header validation and compare HTTP, HTTPS, and HTTPS with ECH. They show what the network can read from SNI and why the Host header is protected inside HTTPS. The references distinguish ESNI (2018), ECH deployment (2023), and RFC 9849 (March 2026).
- **Header compression.** The interactive HPACK comparison shows the bytes and values of HTTP/1.1 and HTTP/2 headers side by side for an initial and repeated request to `www.example.com`. The [HPACK animation](https://slides.podkolzin.consulting/http-evolution/#hpack-animation) shows static entries and dynamic table eviction. The [QPACK animation](https://slides.podkolzin.consulting/http-evolution/#qpack-animation) shows which entries the decoder has and which ones a field section is waiting for. Both animations support playback, pause, and seeking.
- **QUIC.** [Packet loss](https://slides.podkolzin.consulting/http-evolution/#mobile-loss) shows when the application can read stream B over TCP and QUIC. [Connection migration](https://slides.podkolzin.consulting/http-evolution/#quic-migration) follows a single download as it moves from Wi-Fi to two cell towers.
- **Memory.** The [server memory slide](https://slides.podkolzin.consulting/http-evolution/#server-memory) compares PSS in MiB at 0, 1, 64, 256, and 512 open connections. A dash marks a load the demo does not support. See the [managed heap benchmark](benchmarks/connection-memory/) and the [PSS benchmark](benchmarks/connection-memory/servers.md) for methodology and raw data.

The HTTP/0.9 slide includes the complete first server. Slide order and content live in [slides.ts](presentation/src/data/slides.ts), navigation and the table of contents in [src/deck](presentation/src/deck), and interactive diagrams in [src/demos](presentation/src/demos). The animation player passes the time in seconds to each React scene. [SlideContent](presentation/src/slides/SlideContent.tsx) can also be rendered independently of the application.

## Run the first step

```sh
dotnet run --project 01_1991_HTTP0.9_GET
# In another terminal:
printf 'GET /\r\n' | nc 127.0.0.1 8080
# <h1>Hello, HTTP/0.9!</h1>
```

The HTTP/0.9 server reads one GET line, returns HTML without status or headers, and closes the TCP connection. `/missing` returns a “Not found” HTML page.

## Walk through the steps

Follow the numbered directories in this checkout. Each step has a README with a run command and a comparison with its predecessor. For example, run these commands from the repository root.

```sh
dotnet run --project 09_1996_HTTP1.0_SSL3_CLIENT -- --ssl3
diff -ru --exclude=bin --exclude=obj --exclude=README.md 02_1996_HTTP1.0_HEADERS 03_1996_HTTP1.0_HEAD
```

Exit status 1 means `diff` found differences. The step READMEs also show how to compare the specific C# files used by each project, including linked files.

Each step owns its `Program.cs`. Run it with `dotnet run` from its directory. In-memory examples print their results and exit. Server examples listen on loopback until Ctrl+C. Use `dotnet run -- --help` for demos with multiple modes. The final step defaults to HTTP/3 and exposes the earlier server modes through flags.

Shared helpers live in [`common/`](common/). Each historical version of a helper class lives in the step that introduced it. Projects include the appropriate version with `Compile Include` and `Link`.

```xml
<Compile Include="../21_2015_HTTP2_CONTINUATION/Http2ContinuationConnection.cs" Link="Http2ContinuationConnection.cs" />
```

The IDE shows linked files inside each project and opens the relevant source version. Keep `common/` and the numbered directories in the same checkout. To add a historical step, put the new version of a changed file in the new step, update its `.csproj`, and keep the earlier version available to its existing callers.

The projects share `TlsStream`, certificate generation, and `Http2Frames`. TLS 1.3 handshakes and SPKI pinning extend the shared types through partial classes. The HTTP/2 steps add frame handling (16), streamed header and response handling (19), flow control (20), CONTINUATION (21), and TLS with ALPN (29).

The year in each directory marks a milestone for the protocol or feature. The 2021 HTTP/3 directories introduce QUIC v1, the transport used by HTTP/3 in 2022. SSL and TLS appear in the feature names alongside the relevant HTTP version.

| Step | Directory | What it adds |
| --- | --- | --- |
| 01 | [01_1991_HTTP0.9_GET](01_1991_HTTP0.9_GET/) | GET, HTML, and TCP close |
| 02 | [02_1996_HTTP1.0_HEADERS](02_1996_HTTP1.0_HEADERS/) | Status, headers, and Content-Length |
| 02b | [02b_1996_HTTP1.0_CONTENT_ENCODING](02b_1996_HTTP1.0_CONTENT_ENCODING/) | Compressed HTTP/1.0 responses and a separate RFC 7694 request example |
| 03 | [03_1996_HTTP1.0_HEAD](03_1996_HTTP1.0_HEAD/) | HEAD returns response headers without a body |
| 04 | [04_1996_HTTP1.0_SSL3_PRIMITIVES](04_1996_HTTP1.0_SSL3_PRIMITIVES/) | SSL 3.0 record protection, handshake records, certificate, and byte stream |
| 08 | [08_1996_HTTP1.0_SSL3_HTTPS](08_1996_HTTP1.0_SSL3_HTTPS/) | HTTPS with a custom SSL 3.0 handshake |
| 09 | [09_1996_HTTP1.0_SSL3_CLIENT](09_1996_HTTP1.0_SSL3_CLIENT/) | SSL 3.0 client with SPKI pin verification |
| 10 | [10_1997_HTTP1.1_HOST](10_1997_HTTP1.1_HOST/) | The required Host header in HTTP/1.1 |
| 11 | [11_1997_HTTP1.1_KEEP_ALIVE](11_1997_HTTP1.1_KEEP_ALIVE/) | Multiple requests over one TCP connection |
| 12 | [12_1997_HTTP1.1_CHUNKED](12_1997_HTTP1.1_CHUNKED/) | Chunked response transfer |
| 13 | [13_1999_HTTP1.1_TLS10](13_1999_HTTP1.1_TLS10/) | TLS 1.0 PRF, HMAC, and the new Finished message |
| 14 | [14_2002_HTTP1.1_TLS_AES](14_2002_HTTP1.1_TLS_AES/) | AES-CBC support in TLS 1.0 |
| 15 | [15_2010_HTTP1.1_TLS_RENEGOTIATION](15_2010_HTTP1.1_TLS_RENEGOTIATION/) | TLS renegotiation indication |
| 15b | [15b_2011_HTTP1.1_REALTIME](15b_2011_HTTP1.1_REALTIME/) | SSE in an HTTP response stream and WebSocket after Upgrade |
| 16 | [16_2015_HTTP2_FRAMES](16_2015_HTTP2_FRAMES/) | HTTP/2 connection preface, SETTINGS, and frames over TCP |
| 17 | [17_2015_HTTP2_HPACK](17_2015_HTTP2_HPACK/) | HPACK header tables and Huffman string coding |
| 19 | [19_2015_HTTP2_STREAMS](19_2015_HTTP2_STREAMS/) | HTTP/2 GET and HEAD responses on streams |
| 20 | [20_2015_HTTP2_FLOW_CONTROL](20_2015_HTTP2_FLOW_CONTROL/) | HTTP/2 flow-control windows |
| 21 | [21_2015_HTTP2_CONTINUATION](21_2015_HTTP2_CONTINUATION/) | HTTP/2 header block assembly with CONTINUATION frames |
| 22 | [22_2018_HTTP2_TLS13_KEY_EXCHANGE](22_2018_HTTP2_TLS13_KEY_EXCHANGE/) | TLS 1.3 ClientHello, P-256 key exchange, and traffic keys |
| 25 | [25_2018_HTTP2_TLS13_AUTH_RECORDS](25_2018_HTTP2_TLS13_AUTH_RECORDS/) | TLS 1.3 certificate, Finished, and protected records |
| 28 | [28_2018_HTTP2_TLS13_HTTPS](28_2018_HTTP2_TLS13_HTTPS/) | HTTPS over a custom TLS 1.3 stream |
| 29 | [29_2018_HTTP2_ALPN](29_2018_HTTP2_ALPN/) | HTTP/2 over TLS 1.3 with ALPN `h2` |
| 30 | [30_2021_HTTP3_QUIC_PRIMITIVES](30_2021_HTTP3_QUIC_PRIMITIVES/) | QUIC v1 varints, packet format and protection, offsets, ACKs, and transport parameters |
| 36 | [36_2021_HTTP3_QUIC_TLS](36_2021_HTTP3_QUIC_TLS/) | TLS handshake across QUIC encryption levels |
| 37 | [37_2021_HTTP3_QUIC_UDP](37_2021_HTTP3_QUIC_UDP/) | Accepting QUIC sessions with `UdpClient` |
| 38 | [38_2021_HTTP3_QUIC_PTO](38_2021_HTTP3_QUIC_PTO/) | QUIC ACK tracking and probe timeout |
| 39 | [39_2022_HTTP3_QPACK](39_2022_HTTP3_QPACK/) | QPACK header compression and blocked decoding |
| 40 | [40_2022_HTTP3_QUIC_STREAMS](40_2022_HTTP3_QUIC_STREAMS/) | QUIC streams and flow control for HTTP/3 |
| 41 | [41_2022_HTTP3_RESPONSE](41_2022_HTTP3_RESPONSE/) | HTTP/3 GET and HEAD over QUIC, with HEADERS, DATA, and FIN |
| 42 | [42_2022_HTTP3_QUIC_RECOVERY](42_2022_HTTP3_QUIC_RECOVERY/) | QUIC retransmission with new packet numbers |

The examples cover selected milestones. They discuss SSL 2.0 as background and omit implementations of SSL 2.0 and TLS 1.1 and 1.2.

## Run the final step

Run these commands from the repository root. The examples use the final step. For an earlier step, replace the path after `--project`. Run server and client commands in separate terminals.

## HTTP/1.x

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --http
curl --http1.0 -i http://127.0.0.1:8080/
curl --http1.0 -I http://127.0.0.1:8080/
curl --http1.1 --raw -i http://127.0.0.1:8080/chunks
```

The server handles GET and HEAD for `/` and `/chunks` and returns 404 for other paths. HTTP/1.1 requires one non-empty Host header and reuses the TCP connection until `Connection: close`. `/chunks` sends two chunks and a terminating zero-length chunk. A request with a nonempty body gets 400. The server handles one TCP client at a time with a five-second timeout.

### SSE and WebSocket

Start one mode, then open `http://127.0.0.1:8082/` in a browser.

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --sse
# In another terminal, watch the text/event-stream response:
curl -N http://127.0.0.1:8082/events
```

Press Ctrl+C to stop the server, start it in WebSocket mode, and reload the page. The browser sends “Hello!” and shows the server's reply.

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --ws
```

## Legacy SSL 3.0 and TLS 1.0

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --ssl3
# In another terminal, substitute the printed SPKI pin:
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --ssl3-client PIN

# Repeat with TLS 1.0:
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --tls10
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --tls10-client PIN
```

SSL 3.0 listens on TCP port 8442 and TLS 1.0 on port 8441. The custom `TcpClient` checks the certificate pin and Finished. The temporary certificate uses RSA/SHA-1 and the private key stays in memory. The TLS 1.0 example also accepts an independent OpenSSL client using AES128-SHA.

```sh
printf 'GET / HTTP/1.0\r\n\r\n' | openssl s_client -quiet -tls1 -cipher 'AES128-SHA:@SECLEVEL=0' -connect 127.0.0.1:8441
```

Separate steps add AES (2002) and the initial renegotiation indication (2010), without a second handshake. Use `--tls` for TLS 1.3.

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --tls
curl -k --http1.1 -i https://localhost:8443/
```

TLS 1.3 uses AES-128-GCM-SHA256, P-256, ECDSA, ALPN, and HelloRetryRequest. `-k` accepts the demo's temporary self-signed certificate. This example omits resumption, 0-RTT, and KeyUpdate.

## HTTP/2

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --h2
curl --http2-prior-knowledge -i http://127.0.0.1:8081/
# Or use HTTP/2 over TLS 1.3 with ALPN h2:
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --h2 --tls
curl -k --http2 -I https://localhost:8444/
```

Use a curl build with HTTP/2 support. The demo handles the preface, SETTINGS and its ACK, PING, GET and HEAD, odd stream IDs, HPACK with a dynamic table and Huffman coding, CONTINUATION, WINDOW_UPDATE, and RST_STREAM. Requests are supported without bodies.

## HTTP/3

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --h3
# In another terminal, substitute the printed SPKI pin:
google-chrome --user-data-dir="$(mktemp -d)" --origin-to-force-quic-on=127.0.0.1:8445 --ignore-certificate-errors-spki-list=PIN https://127.0.0.1:8445/
```

The server uses UDP port 8445, QUIC v1, TLS 1.3 with ALPN `h3`, CRYPTO and STREAM frames, and control-stream SETTINGS. It supports QPACK, Huffman coding, and GET and HEAD responses with HEADERS, DATA, and FIN. For incoming headers, it permits a dynamic table of up to 4 KiB and one blocked stream. Response fields are encoded as literals. Step 39 shows QPACK blocking and unblocking in the console.

```sh
dotnet run --project 39_2022_HTTP3_QPACK
```

The demo implements ACK ranges, amplification protection, flow control, retransmission with new packet numbers, PTO with backoff, and retirement of old keys. It allows up to 16 bidirectional and 16 unidirectional streams, 64 KiB per receive stream, 1 MiB per connection, and a send window of up to 12,000 bytes. It omits full RFC 9002 adaptive RTT and congestion control, migration, and 0-RTT.

## Verification

Build all steps with `dotnet build HttpEvolution.slnx -c Release`.

- HTTP/1.x and HTTP/2 were checked with curl and raw frames, including flow-control windows and stream cancellation.
- TLS 1.0 and TLS 1.3 were checked with OpenSSL, including certificate and hostname validation and `close_notify`. SSL 3.0 was checked with a pinned client and independent cryptographic test values.
- TLS, QUIC, HPACK, and QPACK bytes were checked against RFC examples.
- Chrome rendered the page over HTTP/3 after loss of Initial, Handshake, and 1-RTT packets and a duplicated Initial.

These implementations cover selected features for local demonstrations.

## References

- [HTTP/0.9](https://www.w3.org/Protocols/HTTP/AsImplemented), [HTTP/1.0](https://www.rfc-editor.org/rfc/rfc1945), and [HTTP/1.1](https://www.rfc-editor.org/rfc/rfc2068)
- [SSL 3.0](https://www.rfc-editor.org/rfc/rfc6101), [TLS 1.0](https://www.rfc-editor.org/rfc/rfc2246), [AES in TLS](https://www.rfc-editor.org/rfc/rfc3268), and [renegotiation indication](https://www.rfc-editor.org/rfc/rfc5746)
- [HTTP/2](https://www.rfc-editor.org/rfc/rfc7540), [HPACK](https://www.rfc-editor.org/rfc/rfc7541), [TLS 1.3](https://www.rfc-editor.org/rfc/rfc8446), and [TLS 1.3 examples](https://www.rfc-editor.org/rfc/rfc8448)
- [QUIC](https://www.rfc-editor.org/rfc/rfc9000), [QUIC cryptographic examples](https://www.rfc-editor.org/rfc/rfc9001), [HTTP/3](https://www.rfc-editor.org/rfc/rfc9114), and [QPACK](https://www.rfc-editor.org/rfc/rfc9204)

## License

The project code and documentation are available under the [MIT License](LICENSE). See [third-party notices](THIRD_PARTY_NOTICES.md) for photo credits and embedded font licenses.
