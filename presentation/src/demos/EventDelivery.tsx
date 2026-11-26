// Portable version of Animations/studio/src/videos/event-delivery/EventDelivery.tsx.
import type { FC } from "react";

import {
    deliveryEvents,
    getLongPollState,
    LongPollClock,
    longPollMessages,
} from "./LongPoll";
const C = {
    bg: "#f4f4ed",
    panel: "#fafbf5",
    ink: "#173c31",
    muted: "#667c70",
    rule: "#cbd7cb",
    lime: "#c6f35b",
};
const FONT = '"Inter Variable", Inter, sans-serif';
type Side = "left" | "right";
type Message = {
    at: number;
    duration: number;
    from: Side;
    rail: "upper" | "lower";
    event?: boolean;
};
type PanelSpec = {
    title: string;
    summary: string;
    upper: string;
    lower: string;
    upperFrom: Side;
    messages: Message[];
    rtc?: boolean;
};

function transmissions(
    times: readonly number[],
    from: Side,
    duration: number,
    event = false,
): Message[] {
    const rail = from === "left" ? "upper" : "lower";
    return times.map((at) => ({ at, duration, from, rail, event }));
}

const pollingRequests = [1, 5, 9, 13, 17];
const pollingMessages = pollingRequests.flatMap((at, index) => {
    const serverReceives = at + 0.7;
    const previousReceipt = index === 0 ? -1 : pollingRequests[index - 1] + 0.7;
    const hasEvent = deliveryEvents.some(
        (time) => previousReceipt < time && time <= serverReceives,
    );
    return [
        ...transmissions([at], "left", 0.7),
        ...transmissions([at + 0.8], "right", 0.85, hasEvent),
    ];
});

const panels: PanelSpec[] = [
    {
        title: "Polling",
        summary: "Клієнт регулярно перевіряє зміни",
        upper: "запит",
        lower: "відповідь",
        upperFrom: "left",
        messages: pollingMessages,
    },
    {
        title: "Long Polling",
        summary: "У цьому прикладі сервер чекає до 4 с",
        upper: "запит",
        lower: "відповідь",
        upperFrom: "left",
        messages: longPollMessages,
    },
    {
        title: "SSE",
        summary: "Один потік подій від сервера",
        upper: "відкрити потік",
        lower: "потік подій",
        upperFrom: "left",
        messages: [
            ...transmissions([0.15], "left", 0.8),
            ...transmissions(
                [1.15, 2.75, 4.35, 5.95, 9.5, 11.1, 12.7, 14.3, 18],
                "right",
                1.15,
            ),
            ...transmissions(deliveryEvents, "right", 1.15, true),
        ],
    },
    {
        title: "WebSocket",
        summary: "Обидві сторони надсилають кадри",
        upper: "Кадр клієнта",
        lower: "Кадр сервера",
        upperFrom: "left",
        messages: [
            ...transmissions(
                [0.4, 2.55, 4.7, 6.85, 9, 11.15, 13.3, 15.45, 17.6, 19.75],
                "left",
                1.25,
            ),
            ...transmissions(
                [1.45, 3.6, 5.75, 10.1, 12.25, 14.4, 18.1],
                "right",
                1.3,
            ),
            ...transmissions(deliveryEvents, "right", 1.25, true),
        ],
    },
    {
        title: "WebRTC",
        summary: "Два клієнти обмінюються даними напряму",
        upper: "дані A → B",
        lower: "дані B → A",
        upperFrom: "left",
        rtc: true,
        messages: [
            ...transmissions([4.25, 6.1, 10.15, 12.35, 17.9], "left", 1.35),
            ...transmissions(deliveryEvents, "left", 1.35, true),
            ...transmissions(
                [5.15, 7.45, 9.45, 12, 16.75, 18.85],
                "right",
                1.4,
            ),
            ...transmissions([14.2], "left", 2.2),
            ...transmissions([14.2], "right", 2.2),
        ],
    },
];

const activity = (seconds: number, messages: Message[], side: Side) =>
    messages.some((m) => {
        const arrival = m.from !== side;
        return (
            seconds >= m.at + (arrival ? m.duration - 0.12 : 0) &&
            seconds < m.at + (arrival ? m.duration + 0.35 : 0.35)
        );
    });

const Monitor: FC<{
    x: number;
    y: number;
    active: boolean;
    label: string;
}> = ({ x, y, active, label }) => (
    <g>
        {active && <circle cx={x} cy={y} r={54} fill={C.lime} opacity={0.65} />}
        <rect
            x={x - 37}
            y={y - 30}
            width={74}
            height={51}
            rx={5}
            fill={C.panel}
            stroke={C.ink}
            strokeWidth={5}
        />
        <path d={`M ${x - 34} ${y - 16} h 68`} stroke={C.ink} strokeWidth={2} />
        <circle cx={x - 25} cy={y - 23} r={2} fill={C.ink} />
        <circle cx={x - 18} cy={y - 23} r={2} fill={C.ink} />
        <line
            x1={x}
            y1={y + 21}
            x2={x}
            y2={y + 33}
            stroke={C.ink}
            strokeWidth={5}
        />
        <line
            x1={x - 21}
            y1={y + 34}
            x2={x + 21}
            y2={y + 34}
            stroke={C.ink}
            strokeWidth={5}
            strokeLinecap="round"
        />
        <text
            x={x}
            y={y + 67}
            textAnchor="middle"
            fontSize={22}
            fontWeight={750}
            fill={C.ink}
        >
            {label}
        </text>
    </g>
);
const Server: FC<{ x: number; y: number; active: boolean }> = ({
    x,
    y,
    active,
}) => (
    <g>
        {active && <circle cx={x} cy={y} r={54} fill={C.lime} opacity={0.65} />}
        {[-28, -6, 16].map((dy) => (
            <g key={dy}>
                <rect
                    x={x - 35}
                    y={y + dy}
                    width={70}
                    height={19}
                    rx={4}
                    fill={C.panel}
                    stroke={C.ink}
                    strokeWidth={4}
                />
                <circle cx={x + 23} cy={y + dy + 9.5} r={3} fill={C.ink} />
            </g>
        ))}
        <text
            x={x}
            y={y + 67}
            textAnchor="middle"
            fontSize={22}
            fontWeight={750}
            fill={C.ink}
        >
            Сервер
        </text>
    </g>
);

const Rail: FC<{
    x1: number;
    x2: number;
    y: number;
    from: Side;
    label: string;
    above: boolean;
    active: boolean;
}> = ({ x1, x2, y, from, label, above, active }) => {
    const end = from === "left" ? x2 : x1;
    const sign = from === "left" ? 1 : -1;
    const color = active ? C.ink : C.muted;
    return (
        <g opacity={active ? 1 : 0.82}>
            <line
                x1={x1}
                y1={y}
                x2={x2}
                y2={y}
                stroke={color}
                strokeWidth={active ? 4 : 3}
            />
            <polygon
                points={`${end},${y} ${end - sign * 15},${y - 9} ${end - sign * 15},${y + 9}`}
                fill={color}
            />
            <text
                x={(x1 + x2) / 2}
                y={y + (above ? -18 : 35)}
                textAnchor="middle"
                fontSize={21}
                fontWeight={650}
                fill={color}
            >
                {label}
            </text>
        </g>
    );
};

const Packet: FC<{
    x1: number;
    x2: number;
    y: number;
    message: Message;
    seconds: number;
}> = ({ x1, x2, y, message, seconds }) => {
    if (seconds < message.at || seconds > message.at + message.duration)
        return null;
    const t = (seconds - message.at) / message.duration;
    const x = message.from === "left" ? x1 + (x2 - x1) * t : x2 - (x2 - x1) * t;
    return (
        <g>
            <circle
                cx={x}
                cy={y}
                r={12}
                fill={C.lime}
                stroke={C.ink}
                strokeWidth={3}
            />
            {message.event && <circle cx={x} cy={y} r={3} fill={C.ink} />}
        </g>
    );
};

const SignalRoute: FC<{
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    start: number;
    seconds: number;
}> = ({ x1, y1, x2, y2, start, seconds }) => {
    const active = seconds >= start && seconds <= start + 0.55;
    const t = Math.max(0, Math.min(1, (seconds - start) / 0.55));
    const angle = Math.atan2(y2 - y1, x2 - x1),
        h = 11,
        w = 6;
    return (
        <g opacity={active ? 1 : 0.35}>
            <line
                x1={x1}
                y1={y1}
                x2={x2}
                y2={y2}
                stroke={C.ink}
                strokeWidth={active ? 4 : 2.5}
            />
            <polygon
                points={`${x2},${y2} ${x2 - h * Math.cos(angle) + w * Math.sin(angle)},${y2 - h * Math.sin(angle) - w * Math.cos(angle)} ${x2 - h * Math.cos(angle) - w * Math.sin(angle)},${y2 - h * Math.sin(angle) + w * Math.cos(angle)}`}
                fill={C.ink}
            />
            {active && (
                <circle
                    cx={x1 + (x2 - x1) * t}
                    cy={y1 + (y2 - y1) * t}
                    r={9}
                    fill={C.lime}
                    stroke={C.ink}
                    strokeWidth={2}
                />
            )}
        </g>
    );
};

const Panel: FC<{
    spec: PanelSpec;
    x: number;
    y: number;
    width: number;
    height: number;
    seconds: number;
}> = ({ spec, x, y, width, height, seconds }) => {
    const left = x + 78,
        right = x + width - 78;
    const actorY = y + (spec.rtc ? 219 : 177);
    const x1 = left + 38,
        x2 = right - 38,
        upperY = actorY - 15,
        lowerY = actorY + 15;
    const isLongPoll = spec.title === "Long Polling";
    const wait = isLongPoll ? getLongPollState(seconds) : undefined;
    const activeRail = (rail: "upper" | "lower") =>
        (rail === "upper" && wait?.waiting === true) ||
        spec.messages.some(
            (m) =>
                m.rail === rail &&
                seconds >= m.at &&
                seconds < m.at + m.duration,
        );
    const latestReply = spec.messages
        .filter((m) => m.from === "right" && m.at <= seconds)
        .reduce<Message | undefined>(
            (last, message) => (!last || message.at > last.at ? message : last),
            undefined,
        );
    const upperLabel = wait?.upperLabel ?? spec.upper;
    let lowerLabel = wait?.lowerLabel ?? spec.lower;
    if (spec.title === "Polling" && latestReply)
        lowerLabel = latestReply.event
            ? "Відповідь із подією"
            : "Поки порожньо";
    if (spec.title === "SSE" && latestReply)
        lowerLabel = latestReply.event ? "Подія" : "Службовий пульс";
    const peerFocus = spec.rtc && seconds >= 12 && seconds < 17;
    const source = spec.rtc ? left : right;
    const signalX = (left + right) / 2,
        signalY = y + 125;
    return (
        <g data-layout-box={`panel-${spec.title}`}>
            <rect
                x={x}
                y={y}
                width={width}
                height={height}
                rx={12}
                fill={C.panel}
                stroke={peerFocus ? C.ink : C.rule}
                strokeWidth={peerFocus ? 3 : 1}
            />
            <text
                x={x + 24}
                y={y + 48}
                fontSize={42}
                fontWeight={780}
                fill={C.ink}
            >
                {spec.title}
            </text>
            <text x={x + 25} y={y + 82} fontSize={23} fill={C.muted}>
                {spec.summary}
            </text>
            {isLongPoll && (
                <LongPollClock
                    x={x + width - 51}
                    y={y + 50}
                    seconds={seconds}
                />
            )}
            {spec.rtc && (
                <g>
                    <SignalRoute
                        x1={left + 25}
                        y1={actorY - 30}
                        x2={signalX - 25}
                        y2={signalY + 17}
                        start={1}
                        seconds={seconds}
                    />
                    <SignalRoute
                        x1={signalX + 25}
                        y1={signalY + 17}
                        x2={right - 25}
                        y2={actorY - 30}
                        start={1.55}
                        seconds={seconds}
                    />
                    <SignalRoute
                        x1={right - 33}
                        y1={actorY - 37}
                        x2={signalX + 33}
                        y2={signalY + 12}
                        start={3}
                        seconds={seconds}
                    />
                    <SignalRoute
                        x1={signalX - 33}
                        y1={signalY + 12}
                        x2={left + 33}
                        y2={actorY - 37}
                        start={3.55}
                        seconds={seconds}
                    />
                    <rect
                        x={signalX - 22}
                        y={signalY - 20}
                        width={44}
                        height={38}
                        rx={4}
                        fill={C.panel}
                        stroke={C.ink}
                        strokeWidth={3}
                    />
                    <line
                        x1={signalX - 20}
                        y1={signalY - 1}
                        x2={signalX + 20}
                        y2={signalY - 1}
                        stroke={C.ink}
                        strokeWidth={3}
                    />
                    <circle
                        cx={signalX + 13}
                        cy={signalY - 10}
                        r={2.5}
                        fill={C.ink}
                    />
                    <circle
                        cx={signalX + 13}
                        cy={signalY + 9}
                        r={2.5}
                        fill={C.ink}
                    />
                    <text
                        x={signalX + 32}
                        y={signalY - 3}
                        fontSize={21}
                        fontWeight={700}
                        fill={C.muted}
                    >
                        Сигнальний сервер
                    </text>
                    {seconds >= 1 && seconds < 2.1 && (
                        <text
                            x={left + 70}
                            y={signalY + 26}
                            textAnchor="middle"
                            fontSize={21}
                            fontWeight={700}
                            fill={C.ink}
                        >
                            пропозиція
                        </text>
                    )}
                    {seconds >= 3 && seconds < 4.1 && (
                        <text
                            x={right - 62}
                            y={signalY + 26}
                            textAnchor="middle"
                            fontSize={21}
                            fontWeight={700}
                            fill={C.ink}
                        >
                            відповідь
                        </text>
                    )}
                </g>
            )}
            {(!spec.rtc || seconds >= 4.2) && (
                <g>
                    <Rail
                        x1={x1}
                        x2={x2}
                        y={upperY}
                        from={spec.upperFrom}
                        label={upperLabel}
                        above
                        active={activeRail("upper")}
                    />
                    <Rail
                        x1={x1}
                        x2={x2}
                        y={lowerY}
                        from={spec.upperFrom === "left" ? "right" : "left"}
                        label={lowerLabel}
                        above={false}
                        active={activeRail("lower")}
                    />
                </g>
            )}
            {spec.messages.map((m, i) => (
                <Packet
                    key={i}
                    x1={x1}
                    x2={x2}
                    y={m.rail === "upper" ? upperY : lowerY}
                    message={m}
                    seconds={seconds}
                />
            ))}
            <Monitor
                x={left}
                y={actorY}
                active={activity(seconds, spec.messages, "left")}
                label={spec.rtc ? "Клієнт A" : "Клієнт"}
            />
            {spec.rtc ? (
                <Monitor
                    x={right}
                    y={actorY}
                    active={activity(seconds, spec.messages, "right")}
                    label="Клієнт B"
                />
            ) : (
                <Server
                    x={right}
                    y={actorY}
                    active={activity(seconds, spec.messages, "right")}
                />
            )}
            {deliveryEvents.some(
                (at) => seconds >= at && seconds < at + 0.9,
            ) && (
                <g transform={`translate(${source} ${actorY - 48}) rotate(45)`}>
                    <rect
                        x={-8}
                        y={-8}
                        width={16}
                        height={16}
                        fill={C.lime}
                        stroke={C.ink}
                        strokeWidth={2}
                    />
                </g>
            )}
        </g>
    );
};

const cues = [
    {
        at: 0,
        text: "У Long Polling відлік починається, коли сервер отримує запит.",
    },
    {
        at: 4.7,
        text: "Минуло 4 секунди. Сервер повертає порожню відповідь, і клієнт запитує знову.",
    },
    {
        at: 8,
        text: "Подія зупиняє годинник раніше. Сервер одразу надсилає її клієнту.",
    },
    {
        at: 10.5,
        text: "У SSE клієнт відкриває потік один раз. Нові події надсилає сервер.",
    },
    {
        at: 13.6,
        text: "Long Polling знову досяг тайм-ауту. Клієнт відкриває наступний запит.",
    },
    {
        at: 15.1,
        text: "У WebRTC два клієнти можуть передавати дані одночасно в обох напрямках.",
    },
    {
        at: 17.4,
        text: "У WebSocket кадрами обмінюються клієнт і сервер. У WebRTC — два клієнти.",
    },
];

export function EventDelivery({ time }: { time: number }) {
    const seconds = Math.min(time, 1199 / 60); // Hold the final frame at the end of playback.
    const phase = cues.reduce(
        (caption, cue) => (seconds >= cue.at ? cue.text : caption),
        cues[0].text,
    );
    return (
        <svg
            style={{
                display: "block",
                width: "100%",
                height: "auto",
                background: C.bg,
                fontFamily: FONT,
            }}
            viewBox="55 110 1810 885"
            role="img"
            aria-label="П’ять способів передачі події між учасниками"
        >
            <Panel
                spec={panels[0]}
                x={70}
                y={130}
                width={572}
                height={288}
                seconds={seconds}
            />
            <Panel
                spec={panels[1]}
                x={674}
                y={130}
                width={572}
                height={288}
                seconds={seconds}
            />
            <Panel
                spec={panels[2]}
                x={1278}
                y={130}
                width={572}
                height={288}
                seconds={seconds}
            />
            <Panel
                spec={panels[3]}
                x={70}
                y={449}
                width={874}
                height={340}
                seconds={seconds}
            />
            <Panel
                spec={panels[4]}
                x={976}
                y={449}
                width={874}
                height={340}
                seconds={seconds}
            />
            <line
                x1={70}
                y1={834}
                x2={1850}
                y2={834}
                stroke={C.rule}
                strokeWidth={2}
            />
            <text x={70} y={895} fontSize={29} fontWeight={700} fill={C.ink}>
                {phase}
            </text>
            <circle
                cx={80}
                cy={956}
                r={10}
                fill={C.lime}
                stroke={C.ink}
                strokeWidth={2}
            />
            <text x={102} y={964} fontSize={22} fill={C.muted}>
                Повідомлення в дорозі
            </text>
            <rect
                x={480}
                y={948}
                width={16}
                height={16}
                transform="rotate(45 488 956)"
                fill={C.lime}
                stroke={C.ink}
                strokeWidth={2}
            />
            <text x={510} y={964} fontSize={22} fill={C.muted}>
                Поява події
            </text>
        </svg>
    );
}
