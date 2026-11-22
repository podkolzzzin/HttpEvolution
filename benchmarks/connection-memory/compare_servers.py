import argparse
import asyncio
import csv
import os
import subprocess
import tempfile
from contextlib import AsyncExitStack, asynccontextmanager
from pathlib import Path

from hpack import HeaderTuple, NeverIndexedHeaderTuple

from run import BODY, HERE, HOST, Http3Client, Mode, batch

PORT = 18080
LEVELS = (64, 256, 512)
WORKERS = 2
SAMPLES = 3
IDLE_SECONDS = 0.7
CLOSED_SECONDS = 2
KESTREL = HERE / "Kestrel" / "bin/Release/net10.0/Kestrel.dll"

MODES = {
    "http11": ("http11", "", 1, None),
    "http11-tls": ("http11-tls", "http/1.1", 1, None),
    "h2": ("h2", "", 1, None),
    "h2-tls": ("h2-tls", "h2", 1, None),
    "h3": ("h3", "h3", 1, None),
    "h3-eight-streams": ("h3", "h3", 8, None),
    "h2-hpack-literal": ("h2-tls", "h2", 1, NeverIndexedHeaderTuple),
    "h2-hpack-indexed": ("h2-tls", "h2", 1, HeaderTuple),
}


def create_certificate(directory):
    command = ["openssl", "req", "-x509", "-newkey", "ec", "-pkeyopt",
               "ec_paramgen_curve:P-256", "-nodes", "-days", "1", "-subj", "/CN=localhost",
               "-keyout", str(directory / "server.key"), "-out", str(directory / "server.crt")]
    subprocess.run(command, check=True, capture_output=True)
    subprocess.run(["openssl", "pkcs12", "-export", "-inkey", str(directory / "server.key"),
                    "-in", str(directory / "server.crt"), "-passout", "pass:",
                    "-out", str(directory / "server.pfx")], check=True)


def worker_pids(pid):
    return tuple(map(int, Path(f"/proc/{pid}/task/{pid}/children").read_text().split()))


def nginx_command(binary, directory, certificate, protocol, tls):
    listen = " quic reuseport" if protocol == "h3" else " ssl" if tls else ""
    http2 = "http2 on;" if protocol in ("h2", "h2-tls") else ""
    ssl = ""
    if tls:
        ssl = f'''
            ssl_certificate "{certificate / 'server.crt'}";
            ssl_certificate_key "{certificate / 'server.key'}";
            ssl_protocols TLSv1.3;
        '''
    body = BODY.decode("ascii").replace("\n", r"\n")
    conf = directory / "nginx.conf"
    conf.write_text(f'''
        daemon off;
        worker_processes {WORKERS};
        pid "{directory / 'nginx.pid'}";
        error_log stderr warn;
        events {{ worker_connections 2048; }}
        http {{
            access_log off;
            client_body_temp_path "{directory / 'client_body'}";
            proxy_temp_path "{directory / 'proxy'}";
            fastcgi_temp_path "{directory / 'fastcgi'}";
            uwsgi_temp_path "{directory / 'uwsgi'}";
            scgi_temp_path "{directory / 'scgi'}";
            server {{
                listen {HOST}:{PORT}{listen};
                {http2}
                {ssl}
                location / {{
                    default_type text/html;
                    return 200 "{body}";
                }}
            }}
        }}
    ''')
    return [str(binary), "-p", str(directory), "-c", str(conf)]


@asynccontextmanager
async def server_process(server, binary, cpus, certificate, mode):
    protocol, alpn, _, _ = MODES[mode]
    with tempfile.TemporaryDirectory(prefix="http-server-") as temporary:
        directory = Path(temporary)
        if server == "kestrel":
            command = ["dotnet", str(KESTREL), protocol, str(PORT), str(certificate / "server.pfx")]
        else:
            command = nginx_command(binary, directory, certificate, protocol, bool(alpn))
        with (directory / "server.log").open("w+") as log:
            process = await asyncio.create_subprocess_exec(
                "taskset", "-c", cpus, *command, stdout=asyncio.subprocess.PIPE, stderr=log,
            )
            try:
                async with asyncio.timeout(10):
                    if server == "kestrel":
                        if await process.stdout.readline() != b"ready\n":
                            raise RuntimeError("Kestrel failed to start")
                        pids = (process.pid,)
                    else:
                        while process.returncode is None:
                            children = worker_pids(process.pid)
                            if len(children) == WORKERS:
                                break
                            await asyncio.sleep(0.02)
                        else:
                            raise RuntimeError("nginx failed to start")
                        pids = (process.pid, *children)
                yield pids
            except Exception:
                log.seek(0)
                if output := log.read():
                    print(output, flush=True)
                raise
            finally:
                if process.returncode is None:
                    process.terminate()
                try:
                    await asyncio.wait_for(process.wait(), timeout=5)
                except TimeoutError:
                    process.kill()
                    await process.wait()


def read_memory(pids):
    totals = {key: 0 for key in ("Rss", "Pss", "Private_Clean", "Private_Dirty")}
    for pid in pids:
        found = set()
        for line in Path(f"/proc/{pid}/smaps_rollup").read_text().splitlines():
            key, _, value = line.partition(":")
            if key in totals:
                totals[key] += int(value.split()[0])
                found.add(key)
        if found != totals.keys():
            raise ValueError(f"Incomplete smaps_rollup for {pid}: {found}")
    return totals["Pss"], totals["Rss"], totals["Private_Clean"] + totals["Private_Dirty"]


async def take_samples(rows, pids, phase, connections, wait=IDLE_SECONDS):
    await asyncio.sleep(wait)
    for number in range(1, SAMPLES + 1):
        rows.append((phase, connections, number, *read_memory(pids)))
        if number < SAMPLES:
            await asyncio.sleep(0.1)


def is_closed(connection):
    if isinstance(connection, Http3Client):
        return connection.closed
    return connection.reader.at_eof() or connection.writer.is_closing()


async def measure(server, binary, cpus, certificate, name):
    protocol, alpn, requests, header = MODES[name]
    mode = Mode(protocol, "", PORT, alpn=alpn, hpack_header=header)
    rows = []
    async with server_process(server, binary, cpus, certificate, name) as pids:
        await take_samples(rows, pids, "ready", 0)
        for _ in range(2):
            async with batch(mode, LEVELS[0], requests=requests, close_transport=True):
                pass
        await take_samples(rows, pids, "warm", 0)
        connections = []
        async with AsyncExitStack() as clients:
            for level in LEVELS:
                opened = await clients.enter_async_context(
                    batch(mode, level - len(connections), requests=requests, close_transport=True)
                )
                connections.extend(opened)
                await take_samples(rows, pids, "idle", level)
                if any(map(is_closed, connections)):
                    raise RuntimeError("A client saw its connection close before the idle snapshot")
        await take_samples(rows, pids, "closed", 0, wait=CLOSED_SECONDS)
    return rows


async def legacy_probes(binary, cpus, certificate, output):
    with output.open("x", newline="") as file:
        writer = csv.writer(file, lineterminator="\n")
        writer.writerow(("server", "request", "server_eof", "response_hex"))
        for server in ("kestrel", "nginx"):
            async with server_process(server, binary, cpus, certificate, "http11"):
                for name, request in (
                    ("http09", b"GET /\r\n"),
                    ("http10", b"GET / HTTP/1.0\r\nHost: 127.0.0.1\r\n\r\n"),
                ):
                    reader, stream = await asyncio.open_connection(HOST, PORT)
                    try:
                        stream.write(request)
                        await stream.drain()
                        received = bytearray()
                        eof = False
                        try:
                            async with asyncio.timeout(2):
                                while chunk := await reader.read(4096):
                                    received.extend(chunk)
                            eof = True
                        except TimeoutError:
                            pass
                        writer.writerow((server, name, int(eof), received.hex()))
                        print(server, name, eof, repr(bytes(received)), flush=True)
                    finally:
                        stream.close()
                        await stream.wait_closed()


async def run(args, certificate):
    if args.legacy_output:
        await legacy_probes(args.nginx, args.server_cpus, certificate, args.legacy_output)
    with args.output.open("x", newline="") as file:
        writer = csv.writer(file, lineterminator="\n")
        writer.writerow(("server", "mode", "trial", "phase", "connections", "sample",
                         "pss_kib", "rss_kib", "uss_kib"))
        for trial in range(1, args.repeats + 1):
            for name in args.mode or MODES:
                for server in args.server or ("kestrel", "nginx"):
                    async with asyncio.timeout(90):
                        rows = await measure(server, args.nginx, args.server_cpus, certificate, name)
                    writer.writerows((server, name, trial, *row) for row in rows)
                    file.flush()
                    last = next(row[3] for row in rows if row[:3] == ("idle", LEVELS[-1], 2))
                    print(f"{trial}: {server} {name}: {last} KiB PSS at {LEVELS[-1]} open", flush=True)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Compare Kestrel and nginx process memory on Linux.")
    parser.add_argument("output", type=Path)
    parser.add_argument("--nginx", type=Path, required=True)
    parser.add_argument("--server-cpus", required=True, help="Two CPUs for the servers, in taskset syntax")
    parser.add_argument("--repeats", type=int, default=5)
    parser.add_argument("--server", action="append", choices=("kestrel", "nginx"))
    parser.add_argument("--mode", action="append", choices=tuple(MODES))
    parser.add_argument("--legacy-output", type=Path)
    args = parser.parse_args()
    if args.repeats < 1:
        parser.error("--repeats must be positive")
    args.nginx = args.nginx.resolve(strict=True)
    if not KESTREL.is_file():
        parser.error("Build Kestrel/Kestrel.csproj in Release first")
    with tempfile.TemporaryDirectory(prefix="http-server-cert-") as scratch:
        certificate = Path(scratch)
        create_certificate(certificate)
        asyncio.run(run(args, certificate))
