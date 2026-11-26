import type { ReactNode } from "react";

/** Portable, seconds-based SSE chat scene for the HttpEvolution presentation. */
export const SSE_CHAT_DURATION = 20;

export type SseChatTimeline = {
  subscribe: { atSeconds: number; durationSeconds: number };
  olyaPost: { atSeconds: number; durationSeconds: number };
  markoDelivery: { atSeconds: number; durationSeconds: number };
  markoPost: { atSeconds: number; durationSeconds: number };
  olyaDelivery: { atSeconds: number; durationSeconds: number };
};

export const SSE_CHAT_TIMELINE: SseChatTimeline = {
  subscribe: { atSeconds: 0.6, durationSeconds: 2.4 },
  olyaPost: { atSeconds: 5, durationSeconds: 1.8 },
  markoDelivery: { atSeconds: 7.3, durationSeconds: 1.8 },
  markoPost: { atSeconds: 11, durationSeconds: 1.8 },
  olyaDelivery: { atSeconds: 13.3, durationSeconds: 1.8 },
} as const;

type Timeline = SseChatTimeline;
type EventKey = keyof Timeline;
type Props = { time: number; timeline?: SseChatTimeline };

const C = {
  bg: "#F4F1EA",
  paper: "#FFFEFA",
  ink: "#151515",
  muted: "#68655F",
  rule: "#D7D1C7",
  coral: "#E96952",
  blue: "#4779A8",
  green: "#4B8A69",
  server: "#E9E4D9",
};
const SANS = "Inter, 'Inter Variable', Arial, sans-serif";
const MONO = "'SFMono-Regular', Consolas, 'Liberation Mono', monospace";
const clamp = (n: number, min = 0, max = 1) => Math.max(min, Math.min(max, n));
const smooth = (n: number) => {
  const t = clamp(n);
  return t * t * (3 - 2 * t);
};
const progress = (time: number, event: Timeline[EventKey]) =>
  smooth((time - event.atSeconds) / event.durationSeconds);
// Match the event boundary at a frame's timestamp despite floating-point rounding.
const ended = (time: number, event: Timeline[EventKey]) =>
  time >= event.atSeconds + event.durationSeconds - 1e-9;
const active = (time: number, event: Timeline[EventKey]) =>
  time >= event.atSeconds && !ended(time, event);

function Label({
  x,
  y,
  children,
  color = C.muted,
  size = 17,
  anchor = "middle",
  mono = false,
  weight = 700,
}: {
  x: number;
  y: number;
  children: ReactNode;
  color?: string;
  size?: number;
  anchor?: "start" | "middle" | "end";
  mono?: boolean;
  weight?: number;
}) {
  return (
    <text x={x} y={y} textAnchor={anchor} fill={color} fontSize={size} fontWeight={weight} fontFamily={mono ? MONO : SANS}>
      {children}
    </text>
  );
}

function Bubble({
  x,
  y,
  text,
  color,
  align,
}: {
  x: number;
  y: number;
  text: string;
  color: string;
  align: "left" | "right";
}) {
  const w = 220;
  return (
    <g>
      <rect x={align === "left" ? x : x - w} y={y} width={w} height={64} rx={18} fill={color} fillOpacity={0.13} stroke={color} strokeWidth={2} />
      <text x={align === "left" ? x + 22 : x - 22} y={y + 49} textAnchor={align === "left" ? "start" : "end"} fill={C.ink} fontFamily={SANS} fontSize={38} fontWeight={750}>
        {text}
      </text>
    </g>
  );
}

function PersonCard({ side, sent, received, subscribed }: { side: "left" | "right"; sent: boolean; received: boolean; subscribed: boolean }) {
  const left = side === "left";
  const x = left ? 60 : 1090;
  const color = left ? C.coral : C.blue;
  const name = left ? "Оля" : "Марко";
  const center = x + 225;
  return (
    <g>
      <rect x={x} y={190} width={450} height={520} rx={26} fill={C.paper} stroke={C.rule} strokeWidth={2} />
      <circle cx={x + 48} cy={240} r={14} fill={color} />
      <Label x={x + 78} y={253} anchor="start" color={C.ink} size={43} weight={800}>{name}</Label>
      <Label x={x + 420} y={245} anchor="end" color={C.muted} size={22} weight={700}>БРАУЗЕР</Label>
      <line x1={x + 28} y1={269} x2={x + 422} y2={269} stroke={C.rule} strokeWidth={2} />
      <Label x={center} y={311} color={C.muted} size={23}>КАНАЛ ПОДІЙ</Label>
      <rect x={center - 177} y={327} width={354} height={54} rx={13} fill={C.bg} />
      <Label x={center} y={363} color={C.ink} size={27} mono>GET /events</Label>
      <g>
        <circle cx={center - 115} cy={418} r={7} fill={subscribed ? C.green : C.muted} />
        <Label x={center - 95} y={426} anchor="start" color={subscribed ? C.green : C.muted} size={21} weight={750}>
          {subscribed ? "потік відкрито" : "очікує підключення"}
        </Label>
      </g>
      <Label x={x + 28} y={484} anchor="start" color={C.muted} size={23}>ЧАТ</Label>
      {sent && (
        <g data-semantic-id={left ? "olya-sent" : "marko-sent"}>
          <Bubble x={x + 422} y={left ? 508 : 590} text={left ? "Привіт!" : "Вітаю!"} color={color} align="right" />
        </g>
      )}
      {received && (
        <g data-semantic-id={left ? "olya-received" : "marko-received"}>
          <Bubble x={x + 28} y={left ? 590 : 508} text={left ? "Вітаю!" : "Привіт!"} color={left ? C.blue : C.coral} align="left" />
        </g>
      )}
    </g>
  );
}

function Packet({ x, y, label, color, width = 190, fontSize = 21 }: { x: number; y: number; label: string; color: string; width?: number; fontSize?: number }) {
  return (
    <g>
      <rect x={x - width / 2} y={y - 28} width={width} height={56} rx={14} fill={C.paper} stroke={color} strokeWidth={2.5} />
      <text x={x} y={y + 7} textAnchor="middle" fill={color} fontFamily={MONO} fontSize={fontSize} fontWeight={800}>{label}</text>
    </g>
  );
}

export function SseChatDiagram({ time, timeline = SSE_CHAT_TIMELINE }: Props) {
  const t = Math.max(0, time);
  const sub = timeline.subscribe;
  const olyaPost = timeline.olyaPost;
  const markoDelivery = timeline.markoDelivery;
  const markoPost = timeline.markoPost;
  const olyaDelivery = timeline.olyaDelivery;
  const subscribed = ended(t, sub);
  const olyaSent = ended(t, olyaPost);
  const markoGot = ended(t, markoDelivery);
  const markoSent = ended(t, markoPost);
  const olyaGot = ended(t, olyaDelivery);
  const streamOpacity = clamp((t - (sub.atSeconds + sub.durationSeconds - 0.35)) / 0.35);
  const olyaPostX = 516 + progress(t, olyaPost) * 134;
  const markoPostX = 1084 - progress(t, markoPost) * 134;
  const markoDataX = 950 + progress(t, markoDelivery) * 124;
  const olyaDataX = 650 - progress(t, olyaDelivery) * 124;
  const getP = progress(t, sub);

  return (
    <svg
      viewBox="0 0 1600 900"
      role="img"
      aria-label="SSE-чат: Оля і Марко відкривають окремі довгі GET-з’єднання, надсилають POST, а сервер передає подію іншому учаснику через його відкритий потік."
      data-scene="sse-chat"
      style={{ width: "100%", height: "auto", display: "block", background: C.bg, fontFamily: SANS }}
    >
      <Label x={64} y={90} anchor="start" color={C.ink} size={58} weight={800}>Шлях повідомлення</Label>
      <Label x={1536} y={82} anchor="end" color={C.muted} size={27}>POST → СЕРВЕР → SSE</Label>
      <line x1={64} y1={112} x2={1536} y2={112} stroke={C.rule} strokeWidth={2} />

      <PersonCard side="left" sent={olyaSent} received={olyaGot} subscribed={subscribed} />
      <PersonCard side="right" sent={markoSent} received={markoGot} subscribed={subscribed} />

      {/* The two server responses remain open independently after subscribe. */}
      <g data-semantic-id="olya-stream" opacity={streamOpacity}>
        <path d="M650 382H516" fill="none" stroke={C.green} strokeWidth={6} strokeLinecap="round" />
        <path d="M516 382l15-10v20z" fill={C.green} />
      </g>
      <g data-semantic-id="marko-stream" opacity={streamOpacity}>
        <path d="M950 382H1084" fill="none" stroke={C.green} strokeWidth={6} strokeLinecap="round" />
        <path d="M1084 382l-15-10v20z" fill={C.green} />
      </g>
      {subscribed && <Label x={800} y={248} color={C.ink} size={27} mono>200 OK · text/event-stream</Label>}

      {/* Subscription requests travel toward the server; one GET per browser. */}
      {active(t, sub) && (
        <>
          <path d="M516 425H650" fill="none" stroke={C.rule} strokeWidth={4} />
          <path d="M650 425l-15-9v18z" fill={C.coral} />
          <path d="M1084 454H950" fill="none" stroke={C.rule} strokeWidth={4} />
          <path d="M950 454l15-9v18z" fill={C.blue} />
          <g data-semantic-id="olya-subscribe"><Packet x={516 + getP * 134} y={425} label="GET" color={C.coral} width={90} /></g>
          <g data-semantic-id="marko-subscribe"><Packet x={1084 - getP * 134} y={454} label="GET" color={C.blue} width={90} /></g>
        </>
      )}

      {/* Server and event fan-out hub. */}
      <g>
        <rect x={650} y={300} width={300} height={410} rx={24} fill={C.server} stroke={C.ink} strokeWidth={2.5} />
        <rect x={775} y={458} width={50} height={50} rx={12} fill={C.ink} />
        <path d="M787 472H813M787 483H813M787 494H806" stroke={C.bg} strokeWidth={3.5} strokeLinecap="round" />
        <Label x={800} y={565} color={C.ink} size={39} weight={800}>Сервер чату</Label>
      </g>

      {/* Incoming SSE events travel outward from the server. */}
      {active(t, markoDelivery) && (
        <>
          <g data-semantic-id="marko-delivery-packet"><Packet x={markoDataX} y={382} label={'data: Привіт!\\n\\n'} color={C.green} width={260} fontSize={20} /></g>
        </>
      )}
      {active(t, olyaDelivery) && (
        <>
          <g data-semantic-id="olya-delivery-packet"><Packet x={olyaDataX} y={382} label={'data: Вітаю!\\n\\n'} color={C.green} width={260} fontSize={20} /></g>
        </>
      )}

      {/* POST requests move toward the server, in their own color-coded lanes. */}
      {active(t, olyaPost) && (
        <g data-semantic-id="olya-post">
          <path d="M516 690H650" fill="none" stroke={C.coral} strokeWidth={4} />
          <path d="M650 690l-15-9v18z" fill={C.coral} />
          <Packet x={olyaPostX} y={690} label="POST" color={C.coral} width={110} />
        </g>
      )}
      {active(t, markoPost) && (
        <g data-semantic-id="marko-post">
          <path d="M1084 690H950" fill="none" stroke={C.blue} strokeWidth={4} />
          <path d="M950 690l15-9v18z" fill={C.blue} />
          <Packet x={markoPostX} y={690} label="POST" color={C.blue} width={110} />
        </g>
      )}
      {active(t, olyaPost) && <Label x={800} y={742} color={C.coral} size={22} mono>POST /messages · Привіт!</Label>}
      {active(t, markoPost) && <Label x={800} y={742} color={C.blue} size={22} mono>POST /messages · Вітаю!</Label>}
    </svg>
  );
}
