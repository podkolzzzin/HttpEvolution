import type { FC } from "react";

export const deliveryEvents = [8, 16.4, 19.7];

const requestTravel = 0.7;
const responseTravel = 0.75;
const timeout = 4;
const reopenDelay = 0.15;
const sceneEnd = 20;

type Message = {
    at: number;
    duration: number;
    from: "left" | "right";
    rail: "upper" | "lower";
    event?: boolean;
};
type Cycle = {
    requestAt: number;
    receivedAt: number;
    replyAt: number;
    event: boolean;
};

// Keep scheduled boundaries on their exact millisecond when converted back to frame time.
const millis = (seconds: number) => Math.round(seconds * 1000) / 1000;
const cycles: Cycle[] = [];
let requestAt = 0;
while (requestAt < sceneEnd) {
    const receivedAt = millis(requestAt + requestTravel);
    const deadline = millis(receivedAt + timeout);
    const nextEvent = deliveryEvents.find(
        (time) => time >= receivedAt && time <= deadline,
    );
    const replyAt = nextEvent ?? deadline;
    cycles.push({
        requestAt,
        receivedAt,
        replyAt,
        event: nextEvent !== undefined,
    });
    requestAt = millis(replyAt + responseTravel + reopenDelay);
}

export const longPollMessages: Message[] = cycles.flatMap((cycle) => [
    {
        at: cycle.requestAt,
        duration: requestTravel,
        from: "left" as const,
        rail: "upper" as const,
    },
    {
        at: cycle.replyAt,
        duration: responseTravel,
        from: "right" as const,
        rail: "lower" as const,
        event: cycle.event,
    },
]);

const currentCycle = (seconds: number): Cycle =>
    [...cycles].reverse().find((cycle) => seconds >= cycle.requestAt) ??
    cycles[0];

export const getLongPollState = (
    seconds: number,
): { upperLabel: string; lowerLabel: string; waiting: boolean } => {
    const cycle = currentCycle(seconds);
    const waiting = seconds >= cycle.receivedAt && seconds < cycle.replyAt;
    return {
        upperLabel: cycle.requestAt === 0 ? "Запит" : "Новий запит",
        lowerLabel:
            seconds >= cycle.replyAt
                ? cycle.event
                    ? "Подія"
                    : "Порожня відповідь"
                : "Відповідь",
        waiting,
    };
};

export const LongPollClock: FC<{
    x: number;
    y: number;
    seconds: number;
}> = ({ x, y, seconds }) => {
    const cycle = currentCycle(seconds);
    const inRequest = seconds < cycle.receivedAt;
    const waiting = seconds >= cycle.receivedAt && seconds < cycle.replyAt;
    const elapsed = inRequest
        ? 0
        : Math.min(
              timeout,
              Math.max(0, Math.min(seconds, cycle.replyAt) - cycle.receivedAt),
          );
    const phase = inRequest
        ? "request"
        : waiting
          ? "waiting"
          : cycle.event
            ? "event"
            : "timeout";
    const status = inRequest
        ? "Запит"
        : waiting
          ? `${elapsed.toFixed(1).replace(".", ",")} / 4 с`
          : cycle.event
            ? "Подія"
            : "Тайм-аут";
    const radius = 30;
    const ringRadius = 26;
    const circumference = 2 * Math.PI * ringRadius;
    const angle = (elapsed / timeout) * Math.PI * 2 - Math.PI / 2;
    return (
        <g data-timer-phase={phase} data-timer-elapsed={elapsed.toFixed(2)}>
            <text
                x={x - 50}
                y={y + 7}
                textAnchor="end"
                fontSize={21}
                fontWeight={700}
                fill="#173c31"
            >
                {status}
            </text>
            <circle
                cx={x}
                cy={y}
                r={radius}
                fill="#f4f4ed"
                stroke="#cbd7cb"
                strokeWidth={2}
            />
            <circle
                cx={x}
                cy={y}
                r={ringRadius}
                fill="none"
                stroke="#c6f35b"
                strokeWidth={5}
                strokeDasharray={`${(circumference * elapsed) / timeout} ${circumference}`}
                transform={`rotate(-90 ${x} ${y})`}
            />
            {Array.from({ length: 12 }, (_, i) => {
                const a = (i * Math.PI) / 6 - Math.PI / 2;
                return (
                    <line
                        key={i}
                        x1={x + 19 * Math.cos(a)}
                        y1={y + 19 * Math.sin(a)}
                        x2={x + 22 * Math.cos(a)}
                        y2={y + 22 * Math.sin(a)}
                        stroke="#173c31"
                        strokeWidth={2}
                        strokeLinecap="round"
                    />
                );
            })}
            <line
                x1={x}
                y1={y}
                x2={x + 16 * Math.cos(angle)}
                y2={y + 16 * Math.sin(angle)}
                stroke="#173c31"
                strokeWidth={3.5}
                strokeLinecap="round"
            />
            <circle cx={x} cy={y} r={3} fill="#173c31" />
        </g>
    );
};
