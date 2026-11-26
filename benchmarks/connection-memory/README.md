# Managed memory per connection

[Slide](https://slides.podkolzin.consulting/http-evolution/#server-memory) · [Raw CSV](../../presentation/public/connection-memory.csv) · [Resident memory comparison across three servers](servers.md)

The measurements were taken on November 21, 2026, using a Release build of commit `fd53db28ada6b517b8a87a4ffb8b5be25e5edfd0`, .NET SDK 10.0.101, and runtime 10.0.1. The host was an AMD Ryzen 5 PRO 4650U running Linux Mint 22.3 (Linux 6.17.0-29-generic, x86-64). The client ran on the same host over IPv4 loopback with Python 3.12.3, h2 4.3.0, and aioquic 1.3.0.

| CSV mode | Step | What happens after the response |
| --- | --- | --- |
| `http09`, `http10` | 01, 02 | The server closes TCP. The client reads to EOF. |
| `http11`, `http11-tls` | 28 with `--http` and in default mode, respectively | The same HTTP/1.1 handler runs without TLS and with TLS 1.3. |
| `h2`, `h2-tls` | 21 and 29 | The same HTTP/2 handler runs without TLS and with TLS 1.3. |
| `h3` | 42 | One `GET /` per QUIC session. Separate series use 1, 8, 16, and 32 clients. |
| `h3-two-streams` | 42 | Two consecutive `GET /` requests on separate streams in the same session before sampling. |
| `h2-hpack-literal`, `h2-hpack-indexed` | 29 | The same 4,000-ASCII-character `x-bench` header is sent with never-indexed encoding and with indexing, respectively. |

## Method

- Each repetition starts a fresh server process with Workstation GC and tiered compilation disabled. Three warm-up batches with the same client count warm up the JIT, cryptography, and shared structures. After closing the warm-up connections and waiting 300 ms, the probe records `baseline_managed_bytes`.
- The client sends `GET /` and verifies the response body and status 200 for protocols with a status line or field. After 300 ms, the probe records `idle_managed_bytes`. A second GET on each persistent connection confirms that it is still usable. After all clients close and another 300 ms elapse, the probe records `closed_managed_bytes`.
- A startup hook runs in the demo process. For each sample it runs a full blocking GC, waits for finalizers, runs another full GC, and records `GC.GetTotalMemory(false)`. This measures live managed objects. Native allocations, kernel memory, and peaks during a handshake or response fall outside the measurement.
- For each row, the per-session value is `(idle_managed_bytes - closed_managed_bytes) / open_after_response`. The slide reports the median of nine repetitions. `baseline_managed_bytes` provides an independent check for drift between snapshots without clients. The HTTP/0.9 and HTTP/1.0 servers close immediately, so there is no open connection to divide by.
- The main comparison uses one client because the TCP demos handle connections synchronously, one at a time. Separate series with more clients measure HTTP/3 scaling.

## Results

The CSV contains 117 rows, with nine measurements for each of 13 scenarios. The median differences between idle and closed for one connection were 3,720 and 4,208 bytes for HTTP/1.1 without TLS and with TLS 1.3, 952 and 1,416 bytes for HTTP/2 without TLS and with TLS 1.3, and 662,592 bytes for HTTP/3. HTTP/0.9 and HTTP/1.0 leave no connection open between requests. For each TCP scenario the difference was identical in all nine repetitions.

With 8, 16, and 32 concurrent QUIC sessions, the medians were 647.71, 647.74, and 647.78 KiB per session, respectively. The difference between medians for a second completed stream was 131,784 bytes (128.70 KiB). Indexing the long HPACK header added 8,096 bytes (7.91 KiB).

## How the implementation affects memory

- HTTP/1.1 keeps a [buffered StreamReader](../../12_1997_HTTP1.1_CHUNKED/Http11ChunkedConnection.cs) between requests. HTTP/2 [reads frames directly](../../16_2015_HTTP2_FRAMES/Http2Connection.cs), and a small request leaves only the `:authority` entry in the [HPACK decoder](../../common/HeaderCompression/Hpack.cs). TLS 1.3 adds managed state and keys in [TlsRecords](../../common/Tls/TlsRecords.cs) and [TlsStream](../../common/Tls/TlsStream.cs).
- HPACK accounts for `x-bench` as 4,039 bytes (7 for the name, 4,000 for the value, and 32 of overhead). In this implementation, that entry retains a UTF-16 string and related objects. The measured difference between the control scenarios is about 8 KiB on the managed heap. [RFC 7541 §4.1](https://www.rfc-editor.org/rfc/rfc7541.html#section-4.1)
- Each [QuicBuffer](../../common/Quic/QuicBuffer.cs) allocates `byte[65536]` and `bool[65536]`. Five buffers remain live after one completed request. Those are the [1-RTT](../../42_2022_HTTP3_QUIC_RECOVERY/QuicSession.cs) `Space.Crypto` buffer and four entries in [QuicStreams.Input](../../common/Quic/QuicStreams.cs). Aioquic opens a control stream, QPACK encoder and decoder streams, and a request stream. The [HTTP/3 handler](../../41_2022_HTTP3_RESPONSE/Http3Connection.cs) marks the request stream complete and retains its buffer until the session closes. The five pairs of arrays total 640 KiB, plus session objects. Another GET on a new stream adds another pair. The demo implementation determines these sizes and lifetimes. [RFC 9114 §6](https://www.rfc-editor.org/rfc/rfc9114.html#section-6)

## Reproduce the benchmark

You need the .NET 10 SDK, Python 3.12, and available loopback ports TCP 8080, 8081, 8443, 8444 and UDP 8445. The clients accept the demos' local self-signed certificates. Run from the repository root.

```sh
dotnet build HttpEvolution.slnx -c Release
python3 -m venv /tmp/http-memory-bench-venv
/tmp/http-memory-bench-venv/bin/pip install -r benchmarks/connection-memory/requirements.txt
/tmp/http-memory-bench-venv/bin/python benchmarks/connection-memory/run.py /tmp/http-memory-results.csv
```
