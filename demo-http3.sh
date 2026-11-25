#!/usr/bin/env bash
set -euo pipefail

project_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
project="$project_dir/42_2022_HTTP3_QUIC_RECOVERY/HttpEvolution.csproj"
curl_bin=${CURL:-curl}

supports_http3() {
    "$1" --disable --version 2>/dev/null | grep -Eq '^Features:.*[[:space:]]HTTP3([[:space:]]|$)'
}

if [[ -z ${CURL:-} ]] && ! supports_http3 "$curl_bin" && command -v brew >/dev/null; then
    if brew_curl_prefix="$(brew --prefix curl 2>/dev/null)"; then
        curl_bin="$brew_curl_prefix/bin/curl"
    fi
fi

if ! supports_http3 "$curl_bin"; then
    printf 'Set CURL to the path of a curl binary with HTTP3 support.\n' >&2
    exit 1
fi

temp_dir="$(mktemp -d)"
server_log="$temp_dir/server.log"
server_pid=

cleanup() {
    local status=$?
    if [[ -n $server_pid ]]; then
        kill "$server_pid" 2>/dev/null || true
        wait "$server_pid" 2>/dev/null || true
    fi
    if (( status != 0 )) && [[ -s $server_log ]]; then
        cat "$server_log" >&2
    fi
    rm -rf -- "$temp_dir"
}

trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM

dotnet build "$project" --nologo --verbosity quiet --output "$temp_dir/build"
dotnet "$temp_dir/build/HttpEvolution.dll" --h3 >"$server_log" 2>&1 &
server_pid=$!

# The server prints its pin after binding the UDP socket.
deadline=$((SECONDS + 10))
pin=
while kill -0 "$server_pid" 2>/dev/null && (( SECONDS < deadline )); do
    pin="$(sed -n 's/^Certificate SPKI pin: //p' "$server_log")"
    if [[ -n $pin ]]; then
        break
    fi
    sleep 0.1
done

if [[ -z $pin ]]; then
    printf 'HTTP/3 server failed to start on 127.0.0.1:8445.\n' >&2
    exit 1
fi

"$curl_bin" --disable --http3-only --noproxy '*' \
    --insecure --pinnedpubkey "sha256//$pin" \
    --silent --show-error --fail-with-body --max-time 10 --include \
    https://127.0.0.1:8445/
