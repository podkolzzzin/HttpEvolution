// Portable version of the Animations sse-event-path composition.
import { HighlightedCode } from "../slides/HighlightedCode";

const readings = [
    { id: 41, value: 21, start: 4 },
    { id: 42, value: 24, start: 15 },
] as const;

const stepStarts = [1, 1.95, 3.12, 4.16, 5.2, 6.2] as const;
const saveComplete = 2.62;
const flushStart = stepStarts[5];
const deliveryComplete = 7.55;

const sampleAt = (seconds: number) => {
    const arrived = readings.filter((reading) => seconds >= reading.start);
    const current = arrived[arrived.length - 1] ?? null;
    const age = current === null ? -1 : seconds - current.start;
    const activeLine =
        age < deliveryComplete
            ? stepStarts.filter((offset) => age + 1e-9 >= offset).length - 1
            : -1;
    const after = (offset: number) =>
        arrived.filter((reading) => seconds >= reading.start + offset);

    return {
        open: seconds >= 2.65,
        current,
        age,
        activeLine,
        stored: after(saveComplete),
        sent: after(flushStart),
        received: after(deliveryComplete),
    };
};

const C = {
    bg: "#f4f4ed",
    ink: "#143b31",
    muted: "#64756c",
    rule: "#b7c8bc",
    paper: "#fffef8",
    lime: "#c2f166",
    terminal: "#102d26",
};
const SANS = "'Inter Variable', 'Inter', Arial, sans-serif";
const MONO = "'IBM Plex Mono', 'SFMono-Regular', Consolas, monospace";
type SceneState = ReturnType<typeof sampleAt>;
const progress = (t: number, a: number, b: number) =>
    Math.max(0, Math.min(1, (t - a) / (b - a)));
const move = (p: number, a: number, b: number) => a + p * (b - a);

function Token({
    x,
    y,
    label,
    hollow = false,
}: {
    x: number;
    y: number;
    label: string;
    hollow?: boolean;
}) {
    const width = Math.max(66, label.length * 14 + 28);
    return (
        <g>
            <rect
                x={x - width / 2}
                y={y - 22}
                width={width}
                height={44}
                rx={22}
                fill={hollow ? C.paper : C.lime}
                stroke={C.ink}
                strokeWidth="2"
            />
            <text
                x={x}
                y={y + 7}
                textAnchor="middle"
                fontFamily={MONO}
                fontSize="19"
                fontWeight="700"
                fill={C.ink}
            >
                {label}
            </text>
        </g>
    );
}

function Schema({ state, time }: { state: SceneState; time: number }) {
    const request = progress(time, 0.45, 1.55);
    const response = progress(time, 1.6, 2.65);
    const reading = state.current;
    const latest = state.received[state.received.length - 1];
    const save = reading ? progress(state.age, 1.95, 2.62) : 0;
    const delivery = reading ? progress(state.age, 6.2, 7.55) : 0;
    return (
        <>
            <div
                style={{
                    position: "absolute",
                    left: 72,
                    top: 48,
                    display: "flex",
                    alignItems: "baseline",
                    gap: 28,
                }}
            >
                <span
                    style={{ fontSize: 30, fontWeight: 800, letterSpacing: 2 }}
                >
                    SSE
                </span>
                <span
                    style={{
                        fontSize: 60,
                        fontWeight: 750,
                        letterSpacing: -1.8,
                    }}
                >
                    Шлях події
                </span>
            </div>
            <div
                style={{
                    position: "absolute",
                    right: 72,
                    top: 65,
                    display: "flex",
                    alignItems: "center",
                    gap: 13,
                    color: C.muted,
                    fontSize: 21,
                }}
            >
                <span
                    style={{
                        width: 12,
                        height: 12,
                        borderRadius: "50%",
                        background: state.open ? C.lime : C.rule,
                        border: `2px solid ${C.ink}`,
                    }}
                />
                {state.open ? "канал відкритий" : "підключення"}
                <span
                    style={{ fontFamily: MONO, color: C.ink, marginLeft: 25 }}
                >
                    GET /events
                </span>
            </div>
            <div
                style={{
                    position: "absolute",
                    left: 72,
                    right: 72,
                    top: 146,
                    height: 2,
                    background: C.rule,
                }}
            />
            <svg
                width="1920"
                height="595"
                style={{ position: "absolute", left: 0, top: 0 }}
            >
                <defs>
                    <marker
                        id="sse-arrow"
                        markerWidth="10"
                        markerHeight="10"
                        refX="9"
                        refY="5"
                        orient="auto"
                    >
                        <path d="M0 0 L10 5 L0 10 z" fill={C.ink} />
                    </marker>
                </defs>
                <path
                    d="M345 325 H555"
                    stroke={C.ink}
                    strokeWidth="3"
                    fill="none"
                    markerEnd="url(#sse-arrow)"
                />
                {time >= 1.6 && (
                    <path
                        d="M800 325 H1385"
                        stroke={C.ink}
                        strokeWidth="3"
                        fill="none"
                        markerEnd="url(#sse-arrow)"
                    />
                )}
                {time < 1.6 && (
                    <path
                        d="M1390 382 H805"
                        stroke={C.rule}
                        strokeWidth="3"
                        fill="none"
                        markerEnd="url(#sse-arrow)"
                    />
                )}
                <text
                    opacity={time < 1.6 ? 0 : 1}
                    x="1095"
                    y="286"
                    textAnchor="middle"
                    fontFamily={MONO}
                    fontSize="22"
                    fill={C.ink}
                >
                    text/event-stream
                </text>
                {request > 0 && request < 1 && (
                    <Token
                        x={move(request, 1308, 883)}
                        y={382}
                        label="GET /events"
                        hollow
                    />
                )}
                {response > 0 && response < 1 && (
                    <Token
                        x={move(response, 849, 1342)}
                        y={325}
                        label="200 OK"
                        hollow
                    />
                )}
                {reading && state.age >= 0 && state.age < 1 && (
                    <Token
                        x={move(state.age, 385, 520)}
                        y={325}
                        label={`${reading.value}°C`}
                    />
                )}
                {reading && delivery > 0 && delivery < 1 && (
                    <Token
                        x={move(delivery, 853, 1337)}
                        y={325}
                        label={`id: ${reading.id}`}
                    />
                )}
                <path
                    d="M680 408 V465"
                    fill="none"
                    stroke={C.rule}
                    strokeWidth="3"
                />
                {reading && state.age >= 1.95 && state.age <= 2.62 && (
                    <circle
                        cx="680"
                        cy={move(save, 408, 465)}
                        r="8"
                        fill={C.lime}
                        stroke={C.ink}
                        strokeWidth="2"
                    />
                )}
            </svg>
            <div
                style={{
                    position: "absolute",
                    left: 72,
                    top: 253,
                    width: 273,
                    height: 156,
                    background: C.ink,
                    color: C.bg,
                    borderRadius: 5,
                    padding: "17px 22px",
                    boxSizing: "border-box",
                    boxShadow: "8px 9px 0 rgba(20,59,49,.12)",
                }}
            >
                <svg width="58" height="55" viewBox="0 0 58 55">
                    <path d="M29 3 52 15 29 27 6 15Z" fill={C.lime} />
                    <path d="M6 15 29 27 29 52 6 40Z" fill="#729d4b" />
                    <path d="M52 15 29 27 29 52 52 40Z" fill="#a1d05b" />
                </svg>
                <div style={{ fontSize: 22, marginTop: 11 }}>
                    Зовнішнє джерело
                </div>
            </div>
            <div
                style={{
                    position: "absolute",
                    left: 560,
                    top: 250,
                    width: 240,
                    height: 158,
                    background: C.paper,
                    border: `3px solid ${C.ink}`,
                    borderRadius: 5,
                    padding: "19px 22px",
                    boxSizing: "border-box",
                }}
            >
                <div style={{ fontSize: 23, fontWeight: 650 }}>Сервер</div>
                <svg
                    width="72"
                    height="58"
                    viewBox="0 0 72 58"
                    style={{ marginTop: 15 }}
                >
                    <rect
                        x="2"
                        y="2"
                        width="68"
                        height="24"
                        rx="4"
                        fill={C.ink}
                    />
                    <rect
                        x="2"
                        y="32"
                        width="68"
                        height="24"
                        rx="4"
                        fill={C.ink}
                    />
                    <circle cx="16" cy="14" r="4" fill={C.lime} />
                    <circle cx="16" cy="44" r="4" fill={C.lime} />
                    <path
                        d="M31 14h27 M31 44h27"
                        stroke={C.bg}
                        strokeWidth="3"
                    />
                </svg>
            </div>
            <div
                style={{
                    position: "absolute",
                    left: 1390,
                    top: 204,
                    width: 458,
                    height: 301,
                    background: C.paper,
                    border: `3px solid ${C.ink}`,
                    borderRadius: 7,
                    boxShadow: "9px 10px 0 rgba(20,59,49,.11)",
                    overflow: "hidden",
                    boxSizing: "border-box",
                }}
            >
                <div
                    style={{
                        height: 47,
                        borderBottom: `2px solid ${C.ink}`,
                        background: "#e6ede4",
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        padding: "0 17px",
                    }}
                >
                    <span
                        style={{
                            fontSize: 18,
                            letterSpacing: 2,
                            color: C.muted,
                        }}
                    >
                        ● ● ●
                    </span>
                    <span
                        style={{ marginLeft: 24, color: C.ink, fontSize: 19 }}
                    >
                        Браузер
                    </span>
                </div>
                <div style={{ padding: "29px 24px" }}>
                    <div
                        style={{
                            fontSize: 19,
                            color: C.muted,
                            letterSpacing: 2,
                            textTransform: "uppercase",
                        }}
                    >
                        Температура
                    </div>
                    <div
                        style={{
                            fontSize: 86,
                            lineHeight: 1,
                            fontWeight: 700,
                            letterSpacing: -4,
                            marginTop: 23,
                        }}
                    >
                        {latest ? `${latest.value} °C` : "—"}
                    </div>
                    {latest && (
                        <div
                            style={{
                                fontFamily: MONO,
                                color: C.muted,
                                fontSize: 20,
                                marginTop: 15,
                            }}
                        >
                            id: {latest.id}
                        </div>
                    )}
                </div>
            </div>
            <div
                style={{
                    position: "absolute",
                    left: 512,
                    top: 465,
                    width: 411,
                    height: 114,
                    border: `2px solid ${C.rule}`,
                    borderRadius: 4,
                    background: C.paper,
                    padding: "13px 19px",
                    boxSizing: "border-box",
                }}
            >
                <div
                    style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 10,
                        fontSize: 18,
                        color: C.muted,
                        letterSpacing: 1.2,
                        textTransform: "uppercase",
                    }}
                >
                    <svg width="22" height="24" viewBox="0 0 22 24">
                        <ellipse
                            cx="11"
                            cy="5"
                            rx="9"
                            ry="4"
                            fill="none"
                            stroke={C.ink}
                            strokeWidth="2"
                        />
                        <path
                            d="M2 5v12c0 5 18 5 18 0V5 M2 11c0 5 18 5 18 0"
                            fill="none"
                            stroke={C.ink}
                            strokeWidth="2"
                        />
                    </svg>
                    База даних
                </div>
                <div
                    style={{
                        display: "flex",
                        gap: 20,
                        fontFamily: MONO,
                        fontSize: 20,
                        marginTop: 12,
                        whiteSpace: "nowrap",
                    }}
                >
                    {state.stored.length ? (
                        state.stored.map((item) => (
                            <span key={item.id}>
                                {item.id} · {item.value}°C
                            </span>
                        ))
                    ) : (
                        <span style={{ color: C.muted }}>очікування</span>
                    )}
                </div>
            </div>
        </>
    );
}

const CODE = [
    { text: "async Task OnReadingAsync(Reading e)", step: 0 },
    { text: "{", step: -1 },
    { text: "  await db.SaveAsync(e);", step: 1 },
    { text: '  await writer.WriteAsync($"id: {e.Id}\\n");', step: 2 },
    { text: '  await writer.WriteAsync($"data: {e.Value}\\n");', step: 3 },
    { text: '  await writer.WriteAsync("\\n");', step: 4 },
    { text: "  await writer.FlushAsync();", step: 5 },
    { text: "}", step: -1 },
];
function CodePanel({ activeLine }: { activeLine: number }) {
    return (
        <div
            className="sse-event-code"
            style={{
                position: "absolute",
                left: 72,
                top: 598,
                width: 1115,
                height: 435,
                border: `2px solid ${C.rule}`,
                borderRadius: 5,
                padding: "20px 24px",
                boxSizing: "border-box",
            }}
        >
            <div
                style={{
                    fontSize: 24,
                    fontWeight: 600,
                    color: "#e9f2e9",
                    marginBottom: 12,
                }}
            >
                Обробник події · C#
            </div>
            {CODE.map(({ text, step }) => (
                <div
                    key={text}
                    style={{
                        height: 43,
                        display: "flex",
                        alignItems: "center",
                        background:
                            step >= 0 && activeLine === step
                                ? "#28492f"
                                : "transparent",
                        margin: "0 -10px",
                        padding: "0 10px",
                        borderRadius: 2,
                        fontFamily: MONO,
                        fontSize: 34,
                        lineHeight: "43px",
                        whiteSpace: "pre",
                    }}
                >
                    <span
                        style={{
                            width: 32,
                            flexShrink: 0,
                            color: C.lime,
                            fontSize: 23,
                        }}
                    >
                        {step >= 0 && activeLine === step ? "›" : ""}
                    </span>
                    <HighlightedCode code={text} />
                </div>
            ))}
        </div>
    );
}
function StreamPanel({ state }: { state: SceneState }) {
    const eventLines = (item: { id: number; value: number }) => [
        `id: ${item.id}`,
        `data: ${item.value}`,
        "",
    ];
    const writing = state.activeLine >= 2 && state.activeLine <= 4;
    const buffer =
        writing && state.current
            ? eventLines(state.current).slice(0, state.activeLine - 1)
            : [];
    const row = (line: string, i: number) => (
        <div
            key={i}
            style={{
                height: 38,
                display: "flex",
                alignItems: "center",
                whiteSpace: "pre",
            }}
        >
            <span
                style={{
                    width: 23,
                    flexShrink: 0,
                    color: "#66877a",
                    fontSize: 17,
                }}
            >
                {line === "" ? "↵" : ""}
            </span>
            <span>{line}</span>
        </div>
    );
    return (
        <div
            style={{
                position: "absolute",
                left: 1210,
                top: 598,
                width: 638,
                height: 435,
                background: C.terminal,
                color: C.bg,
                borderRadius: 5,
                padding: "22px 20px",
                boxSizing: "border-box",
                display: "flex",
                fontFamily: MONO,
                fontSize: 29,
            }}
        >
            <div style={{ flex: 1, minWidth: 0 }}>
                <div
                    style={{
                        fontFamily: SANS,
                        fontSize: 23,
                        color: "#c7dccf",
                        marginBottom: 16,
                    }}
                >
                    Буфер сервера
                </div>
                {buffer.map(row)}
            </div>
            <div
                style={{
                    width: 1,
                    height: "100%",
                    background: "#416054",
                    margin: "0 15px",
                }}
            />
            <div style={{ flex: 1, minWidth: 0 }}>
                <div
                    style={{
                        fontFamily: SANS,
                        fontSize: 23,
                        color: "#c7dccf",
                        marginBottom: 16,
                    }}
                >
                    Потік до браузера
                </div>
                {state.sent.map((event) => (
                    <div
                        key={event.id}
                        style={{
                            width: "fit-content",
                            minWidth: "11ch",
                            border: `2px solid ${C.lime}`,
                            borderRadius: 5,
                            padding: "4px 8px",
                            marginBottom: 10,
                        }}
                    >
                        {eventLines(event).map(row)}
                    </div>
                ))}
            </div>
        </div>
    );
}

export function SseEventPath({ time }: { time: number }) {
    const state = sampleAt(time);
    return (
        <svg
            className="sse-event-path"
            viewBox="0 0 1920 1080"
            style={{ width: "100%", height: "auto" }}
            role="group"
            aria-label="Шлях SSE-події від джерела через сервер до браузера"
        >
            <foreignObject width="1920" height="1080">
                <div
                    style={{
                        position: "relative",
                        width: 1920,
                        height: 1080,
                        overflow: "hidden",
                        background: C.bg,
                        color: C.ink,
                        fontFamily: SANS,
                    }}
                >
                    <Schema state={state} time={time} />
                    <CodePanel activeLine={state.activeLine} />
                    <StreamPanel state={state} />
                </div>
            </foreignObject>
        </svg>
    );
}
