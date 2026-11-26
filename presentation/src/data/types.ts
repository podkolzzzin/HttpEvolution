import type { ReferenceId } from "./references";

export interface TableExample {
    caption: string;
    columns: readonly string[];
    rows: readonly (readonly string[])[];
}

export type InteractiveDemo =
    | "body-gzip"
    | "request-compression"
    | "body-codecs"
    | "ssl-traffic"
    | "host"
    | "frame"
    | "hpack"
    | "loss"
    | "server-memory"
    | "timeline"
    | "resources"
    | "ssl-design"
    | "keepalive"
    | "realtime-years"
    | "ws-chat"
    | "wss"
    | "ws-bytes"
    | "rtc"
    | "queue"
    | "streams"
    | "literal"
    | "repeat"
    | "huffman-bits"
    | "windows"
    | "continuation"
    | "h2-state"
    | "gallery"
    | "tls-changes"
    | "tls-exchange"
    | "tls-finished"
    | "alpn-choice"
    | "udp-stack"
    | "quic-protection"
    | "quic-reliability"
    | "quic-offsets"
    | "quic-flow"
    | "quic-recovery"
    | "operator-limits"
    | "instagram-rollout";
export type Channel = "sse" | "websocket";
export type AnimationKind =
    | "hpack" | "qpack" | "ssl-identity" | "realtime-delivery" | "sse-delivery"
    | "mobile-loss" | "quic-migration" | "sse-chat" | "console" | "tls-exchange";

export interface AnimationChapter {
    at: number;
    label: string;
    detail: string;
}

export interface SlideDefinition {
    id: string;
    year: string;
    chapter: string;
    title: string;
    why: string;
    change?: string;
    changeLabel?: string;
    beats?: readonly { title: string; text: string }[];
    note?: string;
    memory?: string;
    code?: string;
    file?: string;
    codeTitle?: string;
    foldCode?: boolean;
    language?: string;
    codeCaption?: string;
    sourceFile?: string;
    step?: string;
    rfc?: ReferenceId;
    extraRefs?: readonly ReferenceId[];
    client?: string;
    wire?: string;
    command?: string;
    explain?: readonly (readonly [term: string, meaning: string])[];
    example?: TableExample;
    keyRows?: readonly (readonly string[])[];
    interactive?: InteractiveDemo;
    animation?: {
        kind: AnimationKind;
        chapters?: readonly AnimationChapter[];
        media?: { mp4: string; poster?: { src: string; alt: string } };
    };
    channel?: Channel;
    walkthrough?: boolean;
    full?: boolean;
    wide?: boolean;
    points?: readonly string[];
    final?: boolean;
}
