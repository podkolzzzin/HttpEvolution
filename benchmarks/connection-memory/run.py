import argparse
import asyncio
import csv
import itertools
import os
import ssl
import subprocess
import tempfile
from contextlib import AsyncExitStack, asynccontextmanager
from dataclasses import dataclass
from pathlib import Path

from aioquic.asyncio import QuicConnectionProtocol, connect
from aioquic.h3.connection import H3_ALPN, H3Connection
from aioquic.h3.events import DataReceived as H3Data, HeadersReceived
from aioquic.quic.configuration import QuicConfiguration
from aioquic.quic.events import ConnectionTerminated as QuicTerminated
from h2.connection import H2Connection
from h2.events import ConnectionTerminated, DataReceived, ResponseReceived, StreamEnded, StreamReset
from hpack import HeaderTuple, NeverIndexedHeaderTuple

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
HOST = "127.0.0.1"
BODY = b"<h1>Hello, HTTP!</h1>\n"
SETTLE_SECONDS = 0.3
LARGE_HEADER = b"x" * 4000


@dataclass(frozen=True)
class Mode:
    name: str
    project: str
    port: int
    args: tuple = ()
    alpn: str = ""
    closes: bool = False
    hpack_header: type | None = None


HTTP1_TLS = "28_2018_HTTP2_TLS13_HTTPS"
HTTP3 = "42_2022_HTTP3_QUIC_RECOVERY"
MODES = (
    Mode("http09", "01_1991_HTTP0.9_GET", 8080, closes=True),
    Mode("http10", "02_1996_HTTP1.0_HEADERS", 8080, closes=True),
    Mode("http11", HTTP1_TLS, 8080, ("--http",)),
    Mode("http11-tls", HTTP1_TLS, 8443, alpn="http/1.1"),
    Mode("h2", "21_2015_HTTP2_CONTINUATION", 8081),
    Mode("h2-tls", "29_2018_HTTP2_ALPN", 8444, alpn="h2"),
    Mode("h3", HTTP3, 8445, alpn="h3"),
    Mode("h3-two-streams", HTTP3, 8445, alpn="h3"),
    Mode("h2-hpack-literal", "29_2018_HTTP2_ALPN", 8444, alpn="h2",
         hpack_header=NeverIndexedHeaderTuple),
    Mode("h2-hpack-indexed", "29_2018_HTTP2_ALPN", 8444, alpn="h2",
         hpack_header=HeaderTuple),
)


def fields(scheme):
    return [(b":method", b"GET"), (b":scheme", scheme),
            (b":authority", HOST.encode()), (b":path", b"/")]


def verify_response(status, body):
    if status != b"200" or body != BODY:
        raise ValueError(f"Unexpected response: {status!r} {body[:100]!r}")


class Http1Client:
    def __init__(self, reader, writer, mode):
        self.reader = reader
        self.writer = writer
        self.mode = mode

    async def get(self):
        if self.mode.name == "http09":
            self.writer.write(b"GET /\r\n")
            await self.writer.drain()
            if await self.reader.read() != b"<h1>Hello, HTTP/0.9!</h1>\n":
                raise ValueError("Invalid HTTP/0.9 response or missing EOF")
            return
        version = b"HTTP/1.0" if self.mode.closes else b"HTTP/1.1"
        self.writer.write(b"GET / " + version + b"\r\nHost: 127.0.0.1\r\n\r\n")
        await self.writer.drain()
        response = await self.reader.readuntil(b"\r\n\r\n")
        status_line, *header_lines = response.strip().split(b"\r\n")
        if not status_line.startswith(version + b" "):
            raise ValueError(f"Unexpected {status_line!r}")
        headers = dict(line.lower().split(b":", 1) for line in header_lines)
        body = await self.reader.readexactly(int(headers[b"content-length"]))
        verify_response(status_line.split()[1], body)
        if self.mode.closes and await self.reader.read() != b"":
            raise ValueError("Expected EOF after HTTP/1.0 response")


class Http2Client:
    def __init__(self, reader, writer, mode):
        self.reader = reader
        self.writer = writer
        self.mode = mode
        self.h2 = H2Connection()
        self.h2.initiate_connection()

    async def get(self):
        stream = self.h2.get_next_available_stream_id()
        headers = fields(b"https" if self.mode.alpn else b"http")
        if self.mode.hpack_header:
            headers.append(self.mode.hpack_header(b"x-bench", LARGE_HEADER))
        self.h2.send_headers(stream, headers, end_stream=True)
        if self.mode.hpack_header:
            entries = self.h2.encoder.header_table.dynamic_entries
            if ((b"x-bench", LARGE_HEADER) in entries) != (self.mode.hpack_header is HeaderTuple):
                raise ValueError("Client used unexpected HPACK indexing")
        self.writer.write(self.h2.data_to_send())
        await self.writer.drain()
        status = None
        body = bytearray()
        ended = False
        while not ended:
            data = await self.reader.read(65536)
            if not data:
                raise ValueError("HTTP/2 closed before END_STREAM")
            for event in self.h2.receive_data(data):
                if isinstance(event, ResponseReceived):
                    status = dict(event.headers)[b":status"]
                elif isinstance(event, DataReceived):
                    body.extend(event.data)
                    self.h2.acknowledge_received_data(event.flow_controlled_length, event.stream_id)
                elif isinstance(event, StreamEnded) and event.stream_id == stream:
                    ended = True
                elif isinstance(event, (StreamReset, ConnectionTerminated)):
                    raise ValueError(f"HTTP/2 terminated early: {event}")
            self.writer.write(self.h2.data_to_send())
            await self.writer.drain()
        verify_response(status, body)


class Http3Client(QuicConnectionProtocol):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.h3 = H3Connection(self._quic)
        self.events = asyncio.Queue()

    def quic_event_received(self, event):
        if isinstance(event, QuicTerminated):
            self.events.put_nowait(event)
        for received in self.h3.handle_event(event):
            if isinstance(received, (HeadersReceived, H3Data)):
                self.events.put_nowait(received)

    async def get(self):
        stream = self._quic.get_next_available_stream_id()
        self.h3.send_headers(stream, fields(b"https"), end_stream=True)
        self.transmit()
        status = None
        body = bytearray()
        while True:
            event = await self.events.get()
            if isinstance(event, QuicTerminated):
                raise ValueError(f"QUIC closed early: {event}")
            if event.stream_id != stream:
                raise ValueError(f"Unexpected HTTP/3 stream {event.stream_id}")
            if isinstance(event, HeadersReceived):
                status = dict(event.headers)[b":status"]
            elif isinstance(event, H3Data):
                body.extend(event.data)
            if event.stream_ended:
                verify_response(status, body)
                return


@asynccontextmanager
async def client(mode):
    if mode.alpn == "h3":
        config = QuicConfiguration(is_client=True, alpn_protocols=H3_ALPN,
                                   verify_mode=ssl.CERT_NONE)
        async with connect(HOST, mode.port, configuration=config,
                           create_protocol=Http3Client) as connection:
            yield connection
        return
    tls = None
    if mode.alpn:
        tls = ssl.SSLContext(ssl.PROTOCOL_TLS_CLIENT)
        tls.check_hostname = False
        tls.verify_mode = ssl.CERT_NONE
        tls.minimum_version = ssl.TLSVersion.TLSv1_3
        tls.set_alpn_protocols([mode.alpn])
    reader, writer = await asyncio.open_connection(HOST, mode.port, ssl=tls)
    try:
        if tls and writer.get_extra_info("ssl_object").selected_alpn_protocol() != mode.alpn:
            raise ValueError(f"ALPN did not select {mode.alpn}")
        protocol = Http2Client if mode.name.startswith("h2") else Http1Client
        yield protocol(reader, writer, mode)
    finally:
        writer.close()
        await writer.wait_closed()


async def get_all(connections):
    async with asyncio.TaskGroup() as group:
        for connection in connections:
            group.create_task(connection.get())


@asynccontextmanager
async def batch(mode, count):
    stacks = [AsyncExitStack() for _ in range(count)]
    try:
        async with asyncio.TaskGroup() as group:
            tasks = [group.create_task(stack.enter_async_context(client(mode))) for stack in stacks]
        connections = [task.result() for task in tasks]
        await get_all(connections)
        if mode.name == "h3-two-streams":
            await get_all(connections)
        yield connections
    finally:
        await asyncio.gather(*(stack.aclose() for stack in stacks))


async def sample(process, directory):
    result = directory / "result"
    (directory / "sample").touch()
    while not result.exists():
        if process.returncode is not None:
            raise RuntimeError(f"Server exited with code {process.returncode}")
        await asyncio.sleep(0.005)
    value = int(result.read_text())
    result.unlink()
    return value


async def measure(mode, count):
    app = ROOT / mode.project / "bin/Release/net10.0/HttpEvolution.dll"
    hook = HERE / "bin/Release/net10.0/HeapProbe.dll"
    with tempfile.TemporaryDirectory(prefix="http-memory-") as temporary:
        directory = Path(temporary)
        env = dict(os.environ, DOTNET_STARTUP_HOOKS=str(hook), DOTNET_gcServer="0",
                   DOTNET_TieredCompilation="0", HTTP_MEMORY_PROBE_DIRECTORY=temporary)
        with (directory / "stderr").open("w+") as errors:
            process = await asyncio.create_subprocess_exec(
                "dotnet", str(app), *mode.args,
                env=env, stdout=asyncio.subprocess.PIPE, stderr=errors,
            )
            try:
                ready = await process.stdout.readline()
                if f"on {HOST}:{mode.port}".encode() not in ready:
                    raise RuntimeError(f"Server failed to listen: {ready!r}")
                for _ in range(3):
                    async with batch(mode, count) as connections:
                        if not mode.closes:
                            await get_all(connections)
                await asyncio.sleep(SETTLE_SECONDS)
                await sample(process, directory)  # Warm up the measurement thread itself.
                before = await sample(process, directory)
                async with batch(mode, count) as connections:
                    await asyncio.sleep(SETTLE_SECONDS)
                    idle = await sample(process, directory)
                    if not mode.closes:
                        await get_all(connections)  # A response on the same connection proves liveness.
                await asyncio.sleep(SETTLE_SECONDS)
                after = await sample(process, directory)
                errors.seek(0)
                if log := errors.read():
                    raise ValueError(log.strip())
                return before, idle, after
            except Exception:
                errors.seek(0)
                if log := errors.read():
                    print(log.strip(), flush=True)
                raise
            finally:
                if process.returncode is None:
                    process.terminate()
                await process.wait()


async def run(output, repeats, selected):
    cases = [(mode, count) for mode in MODES if not selected or mode.name in selected
             for count in ((1, 8, 16, 32) if mode.name == "h3" else (1,))]
    with output.open("x", newline="") as file:
        writer = csv.writer(file, lineterminator="\n")
        writer.writerow(("mode", "trial", "clients", "open_after_response",
                         "baseline_managed_bytes", "idle_managed_bytes", "closed_managed_bytes"))
        for trial, (mode, count) in itertools.product(range(1, repeats + 1), cases):
            async with asyncio.timeout(30):
                before, idle, after = await measure(mode, count)
            writer.writerow((mode.name, trial, count, 0 if mode.closes else count, before, idle, after))
            file.flush()
            print(f"{trial}: {mode.name} x{count}: {before}, {idle}, {after} "
                  f"(idle − closed: {idle - after:+d})", flush=True)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Measure live managed bytes on the demo servers.")
    parser.add_argument("output", type=Path)
    parser.add_argument("--repeats", type=int, default=9)
    parser.add_argument("--mode", action="append", choices=[mode.name for mode in MODES])
    args = parser.parse_args()
    if args.repeats < 1:
        parser.error("--repeats must be positive")
    subprocess.run(["dotnet", "build", str(HERE / "HeapProbe.csproj"), "-c", "Release", "--nologo"],
                   check=True)
    asyncio.run(run(args.output, args.repeats, args.mode))
