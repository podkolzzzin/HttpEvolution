import argparse
import csv
import json
from collections import defaultdict
from pathlib import Path
from statistics import median

LEVELS = (64, 256, 512)
MODES = ("http11", "http11-tls", "h2", "h2-tls", "h3", "h3-eight-streams",
         "h2-hpack-literal", "h2-hpack-indexed")
SERVERS = ("kestrel", "nginx")
PHASES = {("ready", 0), ("warm", 0), ("closed", 0)} | {("idle", level) for level in LEVELS}


def load_trials(path):
    trials = defaultdict(lambda: defaultdict(dict))
    with path.open(newline="") as source:
        for row in csv.DictReader(source):
            key = (row["server"], row["mode"], int(row["trial"]))
            phase = (row["phase"], int(row["connections"]))
            sample = int(row["sample"])
            if sample in trials[key][phase]:
                raise ValueError(f"Duplicate sample for {key}, {phase}")
            trials[key][phase][sample] = int(row["pss_kib"])
    repeat_count = max(key[2] for key in trials)
    expected = {(server, mode, trial) for server in SERVERS for mode in MODES
                for trial in range(1, repeat_count + 1)}
    if trials.keys() != expected:
        raise ValueError("Missing or unexpected server, mode, or trial")
    for key, phases in trials.items():
        if phases.keys() != PHASES or any(samples.keys() != {1, 2, 3} for samples in phases.values()):
            raise ValueError(f"Missing or unexpected snapshots for {key}")
    return repeat_count, {
        key: {phase: median(samples.values()) for phase, samples in phases.items()}
        for key, phases in trials.items()
    }


def summarize(path):
    repeats, trials = load_trials(path)
    modes = {}
    for mode in MODES:
        modes[mode] = {}
        for server in SERVERS:
            runs = [trials[server, mode, trial] for trial in range(1, repeats + 1)]
            idle = {
                level: [run["idle", level] - run["warm", 0] for run in runs]
                for level in LEVELS
            }
            modes[mode][server] = {
                "warm": median(run["warm", 0] for run in runs),
                "idle": {str(level): median(idle[level]) for level in LEVELS},
                "closed": median(run["closed", 0] - run["warm", 0] for run in runs),
                "range512": [min(idle[512]), max(idle[512])],
            }
    controls = {}
    for name, a, b in (
        ("hpack", "h2-hpack-indexed", "h2-hpack-literal"),
        ("quicStreams", "h3-eight-streams", "h3"),
    ):
        controls[name] = {}
        for server in SERVERS:
            deltas = [
                trials[server, a, trial]["idle", 512] - trials[server, b, trial]["idle", 512]
                for trial in range(1, repeats + 1)
            ]
            controls[name][server] = {
                "median": median(deltas),
                "range": [min(deltas), max(deltas)],
            }
    return {"repeats": repeats, "modes": modes, "controls": controls}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Summarize the process memory benchmark for the slide.")
    parser.add_argument("csv", type=Path)
    args = parser.parse_args()
    print(json.dumps(summarize(args.csv), indent=2))
