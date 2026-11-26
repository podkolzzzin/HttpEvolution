import { useEffect, useRef, useState } from "react";
import type { ComponentType } from "react";
import type {
    AnimationChapter,
    AnimationKind,
    SlideDefinition,
} from "../data/types";
import { HpackDiagram, HPACK_DURATION } from "./HpackDiagram";
import { QpackDiagram, QPACK_DURATION } from "./QpackDiagram";
import { MobileLossDiagram } from "./MobileLossDiagram";
import { MigrationDiagram } from "./MigrationDiagram";
import { SslIdentity } from "./LegacyScenes";
import { SseEventPath } from "./SseEventPath";
import { SseChatDiagram, SSE_CHAT_DURATION } from "./SseChatDiagram";
import { EventDelivery } from "./EventDelivery";
import { ProtocolConsole, PROTOCOL_CONSOLE_DURATION } from "./ProtocolConsole";
import { getTlsExchangeBeat, TlsKeyExchangeDiagram, TLS_EXCHANGE_DURATION } from "./TlsKeyExchangeDiagram";

type Scene = ComponentType<{ time: number }>;
function TlsExchangeScene({ time }: { time: number }) {
    const beat = getTlsExchangeBeat(time);
    return (
        <>
            <TlsKeyExchangeDiagram time={time} />
            <p className="tls-exchange-mobile-caption" aria-hidden="true">
                <strong>{beat.title}</strong>
                {beat.detail}
            </p>
        </>
    );
}

const scenes = {
    hpack: [HpackDiagram, HPACK_DURATION, 22],
    qpack: [QpackDiagram, QPACK_DURATION, 20],
    "ssl-identity": [SslIdentity, 36, 29],
    "tls-exchange": [TlsExchangeScene, TLS_EXCHANGE_DURATION, 53],
    "realtime-delivery": [EventDelivery, 20, 17],
    "sse-delivery": [SseEventPath, 24, 23],
    "sse-chat": [SseChatDiagram, SSE_CHAT_DURATION, SSE_CHAT_DURATION],
    "mobile-loss": [MobileLossDiagram, 16, 10],
    "quic-migration": [MigrationDiagram, 18, 16],
    console: [
        ProtocolConsole,
        PROTOCOL_CONSOLE_DURATION,
        PROTOCOL_CONSOLE_DURATION,
    ],
} satisfies Record<AnimationKind, [Scene, number, number]>;

function AnimationPlayer({
    Scene,
    duration,
    chapters,
    autoPlay,
    revealChapterOnMobile,
}: {
    Scene: Scene;
    duration: number;
    chapters?: readonly AnimationChapter[];
    autoPlay: boolean;
    revealChapterOnMobile: boolean;
}) {
    const stage = useRef<HTMLDivElement>(null);
    const [time, setTime] = useState(0);
    const [playFrom, setPlayFrom] = useState<number | null>(() =>
        autoPlay &&
        !window.matchMedia("(prefers-reduced-motion: reduce)").matches
            ? 0
            : null,
    );

    useEffect(() => {
        if (playFrom === null) return;
        const startTime = playFrom;
        const startedAt = performance.now();
        let frame = requestAnimationFrame(tick);
        function tick(now: number) {
            const nextTime = Math.min(
                duration,
                startTime + (now - startedAt) / 1000,
            );
            setTime(nextTime);
            if (nextTime < duration) frame = requestAnimationFrame(tick);
            else setPlayFrom(null);
        }
        return () => cancelAnimationFrame(frame);
    }, [duration, playFrom]);

    function seek(seconds: number) {
        setPlayFrom(null);
        setTime(seconds);
    }
    function togglePlayback() {
        if (playFrom !== null) setPlayFrom(null);
        else {
            const start = time === duration ? 0 : time;
            setTime(start);
            setPlayFrom(start);
        }
    }

    const selected =
        chapters?.reduce(
            (last, chapter, i) => (time >= chapter.at ? i : last),
            0,
        ) ?? 0;
    return (
        <div className={chapters ? "chapter-player" : undefined}>
            <div className="compression-player" ref={stage}>
                <Scene time={time} />
                <div className="compression-controls">
                    <button type="button" onClick={togglePlayback}>
                        {playFrom !== null
                            ? "Пауза"
                            : time === duration
                              ? "Повторити"
                              : "Відтворити"}
                    </button>
                    <input
                        type="range"
                        min={0}
                        max={duration}
                        step={0.1}
                        value={time}
                        aria-label="Позиція анімації"
                        aria-valuetext={
                            time.toFixed(1).replace(".", ",") +
                            " с із " +
                            duration +
                            " с"
                        }
                        onChange={(event) =>
                            seek(event.currentTarget.valueAsNumber)
                        }
                    />
                    <span aria-hidden="true">
                        {Math.floor(time)} / {duration} с
                    </span>
                </div>
            </div>
            {chapters && (
                <aside className="chapter-side">
                    <h2>Кроки анімації</h2>
                    <div className="chapter-buttons">
                        {chapters.map(({ at, label }, i) => (
                            <button
                                type="button"
                                key={at}
                                aria-pressed={i === selected}
                                onClick={() => {
                                    seek(at);
                                    if (revealChapterOnMobile && window.matchMedia("(max-width: 650px)").matches) {
                                        stage.current?.scrollIntoView({ block: "start", behavior: "instant" });
                                    }
                                }}
                            >
                                <time>
                                    {Math.floor(at / 60)}:
                                    {String(Math.floor(at % 60)).padStart(
                                        2,
                                        "0",
                                    )}
                                </time>
                                {label}
                            </button>
                        ))}
                    </div>
                    <p>{chapters[selected].detail}</p>
                </aside>
            )}
        </div>
    );
}

export function SlideAnimation({
    animation,
    active,
}: {
    animation: NonNullable<SlideDefinition["animation"]>;
    active: boolean;
}) {
    const [Scene, duration, printTime] = scenes[animation.kind];
    const poster = animation.media?.poster;
    return (
        <>
            {active && (
                <AnimationPlayer
                    Scene={Scene}
                    duration={duration}
                    chapters={animation.chapters}
                    autoPlay={animation.kind === "console"}
                    revealChapterOnMobile={animation.kind === "tls-exchange"}
                />
            )}
            <div className="compression-print">
                {poster ? (
                    <img src={poster.src} alt={poster.alt} width="1920" height="1080" style={{width: "100%", height: "auto"}} />
                ) : <Scene time={printTime} />}
            </div>
        </>
    );
}
