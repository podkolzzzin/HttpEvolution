import type { ReferenceId } from "./references";

export interface TableExample {
    caption: string;
    columns: readonly string[];
    rows: readonly (readonly string[])[];
}

export type InteractiveDemo =
    "ssl-traffic" | "host" | "frame" | "hpack" | "loss";
export type Channel = "sse" | "websocket";

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
    channel?: Channel;
    walkthrough?: boolean;
    memoryTimeline?: boolean;
    full?: boolean;
    hero?: boolean;
    final?: boolean;
}
