"""Measure absolute process PSS with one live client, including the teaching servers."""
import argparse
import asyncio
import csv
import json
import tempfile
from collections import defaultdict
from pathlib import Path
from statistics import median

from compare_servers import (
    CLOSED_SECONDS, PORT, SAMPLES, create_certificate, server_process, take_samples,
)
from run import MODES as DEMO_MODES, Mode, batch, get_all

SERVERS = ("demo", "nginx", "kestrel")
PROTOCOLS = ("http11", "http11-tls", "h2", "h2-tls", "h3")
PHASES = {"warm": 0, "idle": 1, "closed": 0}
REPEATS = 5


async def measure(server, name, nginx, cpus, certificate):
    demo = next(mode for mode in DEMO_MODES if mode.name == name)
    mode = demo if server == "demo" else Mode(name, "", PORT, alpn=demo.alpn)
    rows = []
    async with server_process(server, nginx, cpus, certificate, name) as pids:
        for _ in range(2):
            async with batch(mode, 1, requests=2, close_transport=True):
                pass
        await take_samples(rows, pids, "warm", 0)
        async with batch(mode, 1, close_transport=True) as connections:
            await take_samples(rows, pids, "idle", 1)
            await get_all(connections)  # Check a second response on the sampled connection.
        await take_samples(rows, pids, "closed", 0, wait=CLOSED_SECONDS)
    return rows


async def run(args, certificate):
    with args.csv.open("x", newline="") as file:
        writer = csv.writer(file, lineterminator="\n")
        writer.writerow(("server", "mode", "trial", "phase", "connections", "sample",
                         "pss_kib", "rss_kib", "uss_kib"))
        for trial in range(1, REPEATS + 1):
            # Vary server order between trials to spread host drift across implementations.
            offset = (trial - 1) % len(SERVERS)
            for mode in PROTOCOLS:
                for server in SERVERS[offset:] + SERVERS[:offset]:
                    async with asyncio.timeout(30):
                        rows = await measure(server, mode, args.nginx, args.server_cpus, certificate)
                    writer.writerows((server, mode, trial, *row) for row in rows)
                    file.flush()
                    print(f"{trial}/{REPEATS} {mode} {server}", flush=True)


def summarize(path):
    readings = defaultdict(dict)
    with path.open(newline="") as file:
        for row in csv.DictReader(file):
            phase = row["phase"]
            key = row["mode"], row["server"], int(row["trial"]), phase
            sample = int(row["sample"])
            if PHASES[phase] != int(row["connections"]) or sample in readings[key]:
                raise ValueError(f"Invalid or duplicate snapshot: {row}")
            readings[key][sample] = int(row["pss_kib"])
    expected = {(m, s, t, p) for m in PROTOCOLS for s in SERVERS
                for t in range(1, REPEATS + 1) for p in PHASES}
    if readings.keys() != expected or any(set(v) != set(range(1, SAMPLES + 1)) for v in readings.values()):
        raise ValueError("Incomplete or unexpected measurement series")
    modes = {}
    for mode in PROTOCOLS:
        modes[mode] = {}
        for server in SERVERS:
            trials = [
                {phase: median(readings[mode, server, trial, phase].values()) for phase in PHASES}
                for trial in range(1, REPEATS + 1)
            ]
            modes[mode][server] = {
                phase: {
                    "pss": median(t[phase] for t in trials),
                    "delta": median(t[phase] - t["warm"] for t in trials),
                }
                for phase in PHASES
            }
    return {"repeats": REPEATS, "modes": modes}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    subs = parser.add_subparsers(dest="command", required=True)
    capture = subs.add_parser("measure")
    capture.add_argument("csv", type=Path)
    capture.add_argument("--nginx", type=Path, required=True)
    capture.add_argument("--server-cpus", required=True)
    subs.add_parser("summarize").add_argument("csv", type=Path)
    args = parser.parse_args()
    if args.command == "measure":
        args.nginx = args.nginx.resolve(strict=True)
        with tempfile.TemporaryDirectory(prefix="single-pss-cert-") as scratch:
            certificate = Path(scratch)
            create_certificate(certificate)
            asyncio.run(run(args, certificate))
    else:
        print(json.dumps(summarize(args.csv), indent=2))
