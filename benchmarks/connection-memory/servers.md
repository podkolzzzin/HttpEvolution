# Resident memory in the demo server, nginx, and Kestrel

[Slide](https://slides.podkolzin.consulting/http-evolution/#server-memory) · [One client across three servers (CSV)](../../presentation/public/single-server-memory.csv) · [Load up to 512 connections (CSV)](../../presentation/public/server-memory.csv) · [HTTP/0.9 and HTTP/1.0 probes (CSV)](../../presentation/public/server-memory-legacy.csv)

## One client across all three servers

On November 23, 2026, each of the three servers ran five times in each of five HTTP modes. The demo TCP servers handle one client at a time, so the common workload uses one active connection. All runs use the same client, 22-byte response, phases, delays, and PSS sampling. The demo uses its own TLS and QUIC implementations and certificate. nginx and Kestrel use the same temporary certificate. The servers ran one at a time on the same Linux host with CPU affinity `0,2`. The client used `4,6`. The order of the servers alternated between repetitions.

1. Warm up a fresh process with two sequential connections and two GET requests on each. Close both connections, then measure baseline PSS.
2. One client receives the response to `GET /`. Begin measuring PSS after 700 ms. After the last snapshot, the client repeats the GET on the same connection and checks status 200 and the exact response body.
3. The client closes the connection. Begin the last measurement two seconds later. Take three PSS snapshots 100 ms apart in each phase. For nginx, count the master and both workers.
4. Use the median of the three snapshots for each phase of each run. The slide shows the median PSS across five runs in MiB. Δ is the median of the five paired differences from the warmed baseline, also in MiB. PSS includes shared pages, JIT-compiled code, and pools, so small differences can go in either direction.

## Load from 64 to 512 connections in nginx and Kestrel

### Test environment

- Linux Mint 22.3 on a Ryzen 5 PRO 4650U, Linux 6.17.0-29-generic, November 21, 2026. The servers ran one at a time with initial CPU affinity `0,2` on separate physical cores. Some Kestrel threads used the wider `0-11` mask in QUIC mode, which is part of the measured configuration. The client used `4,6` over IPv4 loopback.
- **Kestrel ASP.NET Core 10.0.1**, initialized with `WebApplication.CreateBuilder()` and built in Release with .NET SDK 10.0.101. HTTP/3 used Microsoft libmsquic 2.4.20. The run used the default Server GC, JIT, and buffer pools with logging disabled.
- **nginx 1.30.7** from the official Ubuntu Noble amd64 package, with OpenSSL 3.0.13 and two workers. HTTP/2 and HTTP/3 were enabled as needed, with `reuseport` for HTTP/3. Snapshots include the master and both workers. Access logging was disabled.
- The five main configurations were HTTP/1.1, HTTP/1.1 with TLS 1.3, cleartext HTTP/2 (h2c with prior knowledge), HTTP/2 with TLS 1.3, and HTTP/3. Each server returned the same 22-byte body for `GET /` without a backend, compression, or proxy. Both servers used the same temporary ECDSA P-256 certificate. The client checked every status and response body.

## Sampling and aggregation

1. Start a fresh process for each of five repetitions. Take the first `ready` snapshot after startup. Run two warm-up batches of 64 clients. Close the connections and take the `warm` snapshot.
2. Add clients progressively in the measured run. At 64, 256, and 512 open connections, each newly added client has already received a response and the server waits 700 ms. Take three snapshots 100 ms apart at each level. The client verifies that the transport stays open. After the full response, the client closes TCP directly, including in TLS modes, or sends QUIC CONNECTION_CLOSE. Take the `closed` snapshot two seconds after the clients close. The benchmark does not force a GC.
3. The main metric is PSS from `/proc/PID/smaps_rollup`. It includes resident pages for managed and native code and divides shared pages proportionally between processes. The CSV also stores summed process RSS and USS (`Private_Clean + Private_Dirty`). Summing worker RSS can count shared pages more than once. Kernel network buffers, client memory, peaks during a handshake, and throughput fall outside this test. [Linux smaps_rollup documentation](https://www.kernel.org/doc/html/latest/filesystems/proc.html)
4. Use the median of three snapshots for each phase of each repetition. The slide shows the median absolute PSS across five runs. The table below shows the median of five paired differences, `PSS idle − PSS warm` or `PSS closed − PSS warm`. The CSV stores absolute values. Process allocators can retain pages after TCP or QUIC closes, so the two-second snapshot may differ from measurements after a longer idle period.
5. The HPACK control sends the same `x-bench` value of 4,000 ASCII characters without indexing and, in a separate run, with indexing. The HTTP/3 control compares eight sequential completed streams with one. For these controls, compare absolute PSS at 512 open connections between matching run numbers. This measures the process as a whole. Identifying individual objects, the actual memory used by the dynamic table, or the cause of a difference requires separate profiling.

## Results

The data contains 80 independent runs, 1,440 raw process snapshots, and four legacy-protocol probes. The table shows median changes in PSS from the warmed server, in MiB.

| Protocol | Kestrel, 512 open | nginx, 512 open | Kestrel, 2 s after close | nginx, 2 s after close |
| --- | ---: | ---: | ---: | ---: |
| HTTP/1.1 | +6.98 | +0.22 | +7.43 | +0.20 |
| HTTP/1.1 with TLS 1.3 | +26.88 | +6.44 | +26.82 | +6.42 |
| Cleartext HTTP/2 | +22.73 | +3.11 | +23.36 | +3.09 |
| HTTP/2 with TLS 1.3 | +42.92 | +7.78 | +43.33 | +7.78 |
| HTTP/3 | +55.56 | +45.36 | +102.00 | +44.66 |

TLS increased PSS for HTTP/1.1 and HTTP/2 on both servers. HTTP/3 had the largest increase among the main configurations. Its warmed baseline was 85.57 MiB in Kestrel and 15.59 MiB in nginx. Kestrel's PSS was higher two seconds after the HTTP/3 clients closed than with the clients still open. Testing for long-term retention or a leak would require longer observation and profiling.

Indexing the long header changed PSS by +2.83 MiB in Kestrel (+0.11 to +4.28) and +0.12 MiB in nginx (+0.02 to +0.19). Eight sequential HTTP/3 streams instead of one changed absolute PSS by +8.43 MiB in Kestrel (+4.97 to +15.18) and −1.42 MiB in nginx (−2.14 to −0.72). Those differences are between processes with different workload histories, so they cannot isolate the cost of a single stream.

## What the implementation and specifications explain

- TLS adds encryption and connection state to HTTP/1.1 and HTTP/2. HTTP/3 includes TLS 1.3 and QUIC state such as ACKs and flow control. Kestrel handles some QUIC work in native MsQuic, which a managed-heap metric would miss. [Kestrel HTTP/3](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-10.0)
- Flow-control window and buffer settings define limits. PSS measures resident pages. For example, nginx documents a default 64 KiB HTTP/3 read and write buffer. The setting alone does not establish 64 KiB of resident memory per completed stream. [nginx HTTP/3 module](https://nginx.org/en/docs/http/ngx_http_v3_module.html#http3_stream_buffer_size)
- nginx reserves arrays for connections and events when a worker starts. The small HTTP/1.1 increase is measured against a warmed baseline that already includes some of those structures. [nginx worker initialization](https://github.com/nginx/nginx/blob/release-1.30.7/src/event/ngx_event.c#L696-L740)
- In both HPACK scenarios, the client indexes `:authority`. nginx already allocates 4 KiB for the dynamic table on the first entry, so the second, larger header may fit in that existing storage. HPACK accounts for `x-bench` as 4,039 bytes, including 32 bytes of overhead. [nginx HPACK implementation](https://github.com/nginx/nginx/blob/release-1.30.7/src/http/v2/ngx_http_v2_table.c#L175-L204) · [RFC 7541 §4.1](https://www.rfc-editor.org/rfc/rfc7541.html#section-4.1)
- Kestrel reuses pooled pages and schedules their release separately from connection shutdown. For example, the pinned memory pool checks every 10 seconds by default. A single PSS snapshot cannot attribute retention to a specific pool or diagnose a leak. [Kestrel 10.0.1 memory pool implementation](https://github.com/dotnet/aspnetcore/blob/v10.0.1/src/Shared/Buffers.MemoryPool/PinnedBlockMemoryPool.cs)
- Short legacy probes check wire behavior independently. Kestrel returns 400 to an HTTP/0.9 request. nginx returns the body without headers. After a regular HTTP/1.0 request without keep-alive, both returned the body and closed TCP. The probe CSV records the exact response bytes in hex and whether EOF occurred.
- The [managed memory benchmark](README.md) measures live managed objects after a full GC in the demo. This benchmark measures whole-server PSS on Linux. The managed heap snapshots are in a [separate CSV](../../presentation/public/connection-memory.csv).

## Reproduce the benchmark

You need Linux, Python 3.12, the .NET 10 SDK, OpenSSL, an nginx build with `--with-http_v2_module` and `--with-http_v3_module`, and libmsquic. The application listens on loopback port 18080 over TCP and UDP. The native dependencies came from the [nginx package repository](https://nginx.org/packages/ubuntu/pool/nginx/n/nginx/) and the [Microsoft package repository](https://packages.microsoft.com/ubuntu/24.04/prod/pool/main/libm/libmsquic/). If you unpack libmsquic locally, put its directory on `LD_LIBRARY_PATH`. The commands below put the client and server on different CPU pairs on the test host.

```sh
dotnet build benchmarks/connection-memory/Kestrel/Kestrel.csproj -c Release
python3 -m venv /tmp/http-memory-bench-venv
/tmp/http-memory-bench-venv/bin/pip install -r benchmarks/connection-memory/requirements.txt
taskset -c 4,6 /tmp/http-memory-bench-venv/bin/python \
  benchmarks/connection-memory/compare_servers.py /tmp/server-memory.csv \
  --nginx "$(command -v nginx)" --server-cpus 0,2 \
  --legacy-output /tmp/server-memory-legacy.csv \
  --mode h2-tls --mode h3 --mode h3-eight-streams --mode http11 \
  --mode http11-tls --mode h2 --mode h2-hpack-literal --mode h2-hpack-indexed
python3 benchmarks/connection-memory/summarize_servers.py /tmp/server-memory.csv > /tmp/serverMemory.json
```

Use the same environment and free demo ports (TCP 8080, 8081, 8443, and 8444 and UDP 8445) for the single-client comparison. `summarize` validates the completeness of the run and writes the JSON used by the slide.

```sh
dotnet build HttpEvolution.slnx -c Release
taskset -c 4,6 /tmp/http-memory-bench-venv/bin/python \
  benchmarks/connection-memory/compare_single.py measure /tmp/single-server-memory.csv \
  --nginx "$(command -v nginx)" --server-cpus 0,2
/tmp/http-memory-bench-venv/bin/python benchmarks/connection-memory/compare_single.py \
  summarize /tmp/single-server-memory.csv > /tmp/singleServerMemory.json
```
