# SSE and WebSocket over HTTP/1.1

This example shows two ways to exchange messages with a browser. Choose a mode, open the [demo page](http://127.0.0.1:8082/), and inspect the requests in the Network tab. Run the commands from the repository root.
Both modes listen on loopback port 8082. Press Ctrl+C before switching modes.

## SSE server events

```sh
dotnet run --project 15b_2011_HTTP1.1_REALTIME -- --sse
# In another terminal
curl -iN http://127.0.0.1:8082/events
```

The `text/event-stream` response delivers three events one second apart. The first event contains two fields and ends with an empty line.

```text
id: 1
data: tick 1

```

[ServerSentEvents.cs](ServerSentEvents.cs) encodes each event in UTF-8 and sends it in a separate HTTP chunk. An empty line ends the event, and a zero-length chunk ends the response. The page opens `EventSource("/events")` and displays each event as it arrives.
If the connection drops, the browser can resume from the last received ID. Run `curl -iN -H 'Last-Event-ID: 2' http://127.0.0.1:8082/events` to receive only `tick 3`. On a reconnect after ID 3, the server returns `204 No Content`.

## Two-way text over WebSocket

```sh
dotnet run --project 15b_2011_HTTP1.1_REALTIME -- --ws
```

Open the [demo page](http://127.0.0.1:8082/). The browser connects to `/chat`, sends “Hello!”, and shows the reply. Open the `chat` request in the Network tab. Headers shows `101 Switching Protocols`, and Messages shows the outgoing text and its echo.

[WebSocketEcho.cs](WebSocketEcho.cs) computes `Sec-WebSocket-Accept` and switches from HTTP headers to WebSocket frames. The server unmasks client frames, reassembles fragmented text messages, and echoes each message in one frame. It also responds to Ping and Close. Messages are limited to 4,096 UTF-8 bytes. Like the other HTTP/1 demos, the server handles one connection at a time and stops reading after five seconds without data.

See the [SSE specification](https://html.spec.whatwg.org/multipage/server-sent-events.html) and [RFC 6455](https://www.rfc-editor.org/rfc/rfc6455) for the wire formats.

## Building on HTTP/1.1

Compare the event-writing loop with the [`/chunks` response from step 12](../12_1997_HTTP1.1_CHUNKED/Http11ChunkedConnection.cs).
Run this comparison from the repository root to see how the available modes changed after step 15.

```sh
diff -u 15_2010_HTTP1.1_TLS_RENEGOTIATION/Program.cs 15b_2011_HTTP1.1_REALTIME/Program.cs
```

[All steps](../README.md).
