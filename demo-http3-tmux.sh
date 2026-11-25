#!/usr/bin/env bash
set -eu
cd "$(dirname "$0")"
project="$PWD/demos/Http3Streams"
unset TMUX
mux=(tmux -L http3-observatory -f "$project/observatory.tmux.conf")

if ! "${mux[@]}" has-session -t '=observatory' 2>/dev/null; then
    dotnet_bin=$(command -v dotnet)
    curl_bin=${CURL:-curl}
    if [[ -z ${CURL:-} ]] && ! "$curl_bin" -V | grep -qw HTTP3 && command -v brew >/dev/null; then
        curl_bin="$(brew --prefix curl)/bin/curl"
    fi
    if ! "$curl_bin" -V | grep -qw HTTP3; then
        echo 'Для демо потрібен curl із підтримкою HTTP/3.' >&2
        exit 1
    fi

    "$dotnet_bin" build "$project" -v quiet
    dll="$project/bin/Debug/net10.0/Http3Streams.dll"
    "${mux[@]}" new-session -d -s observatory -n HTTP3 -x 110 -y 30 "$dotnet_bin" "$dll" server
    trap '"${mux[@]}" kill-session -t observatory 2>/dev/null || true' EXIT

    # The server prints the certificate fingerprint once its UDP socket is bound.
    for ((i=0; i<60; i++)); do
        pin=$("${mux[@]}" capture-pane -pJ -S - -t observatory:0.0 | awk '/^dotnet run .* -- client / {print $NF; exit}')
        [[ $pin =~ ^[0-9A-Fa-f]{64}$ ]] && break
        [[ $("${mux[@]}" display-message -p -t observatory:0.0 '#{pane_dead}') == 1 ]] && break
        sleep 0.1
    done
    if [[ ! $pin =~ ^[0-9A-Fa-f]{64}$ ]]; then
        "${mux[@]}" capture-pane -pJ -t observatory:0.0 >&2
        echo 'Сервер демо не запустився на UDP-порту 8456.' >&2
        exit 1
    fi

    "${mux[@]}" split-window -dv -t observatory:0.0 "$dotnet_bin" "$dll" client "$pin"
    "${mux[@]}" split-window -dv -t observatory:0.1 "$curl_bin" --disable --noproxy '*' --http3-only -ksiS --max-time 5 https://127.0.0.1:8456/moon
    "${mux[@]}" select-layout -t observatory:0 even-vertical >/dev/null
    "${mux[@]}" select-pane -t observatory:0.0 -T 'Сервер  ·  UdpClient  ·  127.0.0.1:8456'
    "${mux[@]}" select-pane -t observatory:0.1 -T 'Наш клієнт  ·  три потоки, одне QUIC-з’єднання'
    "${mux[@]}" select-pane -t observatory:0.2 -T 'curl --http3-only  ·  GET /moon'
    "${mux[@]}" select-pane -t observatory:0.0
    trap - EXIT
fi

if [[ -t 0 && -t 1 ]]; then
    exec "${mux[@]}" attach-session -t observatory
fi
echo 'tmux -L http3-observatory attach -t observatory'
