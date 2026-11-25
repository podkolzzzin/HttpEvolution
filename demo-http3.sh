#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"

project=42_2022_HTTP3_QUIC_RECOVERY
dotnet build "$project" -v quiet
dotnet "$project/bin/Debug/net10.0/HttpEvolution.dll" --h3 &
server=$!
trap 'kill "$server" 2>/dev/null || true; wait "$server" 2>/dev/null || true' EXIT

sleep 1
kill -0 "$server"
curl --http3-only -sSki --max-time 5 https://127.0.0.1:8445/
