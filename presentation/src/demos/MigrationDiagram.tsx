import type { CSSProperties } from "react";

const colors = {
  bg: "#08111f",
  panel: "#0d1a2b",
  line: "#36506b",
  ink: "#edf5ff",
  muted: "#b1c3d6",
  blue: "#62b5ff",
  teal: "#4ce0c1",
  amber: "#ffbf69",
  purple: "#c5a8ff",
};
const clamp = (value: number, min: number, max: number) =>
  Math.max(min, Math.min(max, value));
const ease = (value: number) => {
  const t = clamp(value, 0, 1);
  return t * t * (3 - 2 * t);
};
const baseText: CSSProperties = {
  fontFamily: "Inter, ui-sans-serif, system-ui, sans-serif",
  fontSize: 15,
  lineHeight: 1.5,
};

function Router({ x, y, color }: { x: number; y: number; color: string }) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <rect
        x={-42}
        y={-25}
        width={84}
        height={50}
        rx={11}
        fill={colors.panel}
        stroke={color}
        strokeWidth={4}
      />
      <path
        d="M-23 1Q0-21 23 1M-14 9Q0-4 14 9"
        fill="none"
        stroke={color}
        strokeWidth={4}
        strokeLinecap="round"
      />
      <circle cx={0} cy={16} r={4} fill={color} />
    </g>
  );
}

function Tower({ x, y, color }: { x: number; y: number; color: string }) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <path
        d="M-31 30L0-37 31 30M-21 7H21M-27 20H27M0-36V30"
        fill="none"
        stroke={color}
        strokeWidth={4}
        strokeLinejoin="round"
      />
      <path
        d="M-17-36Q-42-19-17-2M17-36Q42-19 17-2"
        fill="none"
        stroke={color}
        strokeWidth={3}
      />
    </g>
  );
}

function Server({ x, y }: { x: number; y: number }) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <rect
        x={-38}
        y={-37}
        width={76}
        height={74}
        rx={11}
        fill={colors.panel}
        stroke={colors.purple}
        strokeWidth={4}
      />
      {[-18, 0, 18].map((lineY) => (
        <g key={lineY}>
          <line
            x1={-21}
            y1={lineY}
            x2={23}
            y2={lineY}
            stroke={colors.purple}
            strokeWidth={3}
          />
          <circle cx={-27} cy={lineY} r={3} fill={colors.teal} />
        </g>
      ))}
    </g>
  );
}

function Phone({ x, y }: { x: number; y: number }) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <rect
        x={-20}
        y={-33}
        width={40}
        height={66}
        rx={9}
        fill={colors.bg}
        stroke={colors.ink}
        strokeWidth={4}
      />
      <rect
        x={-13}
        y={-24}
        width={26}
        height={44}
        rx={3}
        fill={colors.blue}
        fillOpacity={0.16}
      />
      <circle cx={0} cy={26} r={2.5} fill={colors.ink} />
    </g>
  );
}

function NetworkDesktop({
  phase,
  phoneX,
  transfer,
}: {
  phase: number;
  phoneX: number;
  transfer: number;
}) {
  const activeStop = phase === 0 ? 0 : phase <= 2 ? 1 : 2;
  const activeX = phase === 0 ? 220 : phase <= 2 ? 610 : 1000;
  const pathColor =
    phase === 0
      ? colors.blue
      : phase === 1 || phase === 3 || phase === 4
        ? colors.amber
        : colors.teal;
  const path =
    phase === 0
      ? "M220 160V222H1280V160"
      : phase <= 2
        ? "M610 160V222H1280V160"
        : "M1000 160V222H1280V160";
  return (
    <svg
      viewBox="0 0 1400 455"
      role="img"
      aria-label="Телефон переходить від Wi-Fi до вежі A та вежі B; кожен новий маршрут зіставляється з тим самим станом QUIC на сервері."
      style={{ display: "block", width: "100%", height: "auto" }}
    >
      <text
        x={220}
        y={35}
        fill={colors.muted}
        textAnchor="middle"
        fontSize={21}
        fontWeight={800}
      >
        WI‑FI РОУТЕР
      </text>
      <text
        x={610}
        y={35}
        fill={colors.muted}
        textAnchor="middle"
        fontSize={21}
        fontWeight={800}
      >
        СТІЛЬНИКОВА ВЕЖА A
      </text>
      <text
        x={1000}
        y={35}
        fill={colors.muted}
        textAnchor="middle"
        fontSize={21}
        fontWeight={800}
      >
        СТІЛЬНИКОВА ВЕЖА B
      </text>
      <Router x={220} y={112} color={phase === 0 ? colors.blue : colors.line} />
      <Tower
        x={610}
        y={112}
        color={phase >= 1 && phase <= 2 ? colors.amber : colors.line}
      />
      <Tower x={1000} y={112} color={phase >= 3 ? colors.teal : colors.line} />
      <Server x={1280} y={112} />
      <text
        x={1280}
        y={169}
        fill={colors.ink}
        textAnchor="middle"
        fontSize={19}
        fontWeight={800}
      >
        СЕРВЕР
      </text>
      <path
        d="M220 160V222H1280V160"
        fill="none"
        stroke={colors.line}
        strokeWidth={4}
        strokeDasharray="10 10"
      />
      <path
        d={path}
        fill="none"
        stroke={pathColor}
        strokeWidth={7}
        strokeLinecap="round"
      />
      <circle
        cx={activeX}
        cy={222}
        r={10}
        fill={
          phase === 0
            ? colors.blue
            : phase === 1 || phase === 3 || phase === 4
              ? colors.amber
              : colors.teal
        }
      />
      <text
        x={760}
        y={211}
        textAnchor="middle"
        fill={colors.ink}
        fontSize={18}
        fontWeight={750}
      >
        АКТИВНИЙ ШЛЯХ ДО СЕРВЕРА
      </text>
      <text x={110} y={285} fill={colors.muted} fontSize={18} fontWeight={800}>
        ШЛЯХ ТЕЛЕФОНА
      </text>
      <line
        x1={220}
        y1={365}
        x2={1000}
        y2={365}
        stroke={colors.line}
        strokeWidth={5}
        strokeLinecap="round"
      />
      <line
        x1={220}
        y1={365}
        x2={phoneX}
        y2={365}
        stroke={colors.blue}
        strokeWidth={5}
        strokeLinecap="round"
      />
      {[220, 610, 1000].map((x, index) => (
        <g key={x}>
          <circle
            cx={x}
            cy={365}
            r={9}
            fill={index === activeStop ? colors.teal : colors.line}
          />
          <text
            x={x}
            y={425}
            fill={index === activeStop ? colors.teal : colors.muted}
            textAnchor="middle"
            fontSize={18}
            fontWeight={800}
          >
            {index === 0 ? "Wi‑Fi" : `ВЕЖА ${index === 1 ? "A" : "B"}`}
          </text>
        </g>
      ))}
      <line x1={phoneX} y1={325} x2={activeX} y2={232} stroke={pathColor} strokeWidth={3} strokeDasharray="6 8" />
      <Phone x={phoneX} y={365} />
      <g>
        <rect
          x={1100}
          y={275}
          width={230}
          height={110}
          rx={14}
          fill="#091423"
          stroke={colors.purple}
          strokeWidth={2}
        />
        <text
          x={1120}
          y={305}
          fill={colors.muted}
          fontSize={15}
          fontWeight={800}
        >
          CID ОДЕРЖУВАЧА
        </text>
        <text
          x={1120}
          y={342}
          fill={colors.purple}
          fontSize={27}
          fontWeight={900}
        >
          {phase === 0 ? "0xA7" : phase <= 2 ? "0xB2" : "0xC4"}
        </text>
        <text x={1120} y={370} fill={colors.ink} fontSize={17} fontWeight={800}>
          → стан 7F3A
        </text>
      </g>
      <rect
        x={110}
        y={440}
        width={1220}
        height={12}
        rx={6}
        fill={colors.line}
      />
      <rect
        x={110}
        y={440}
        width={1220 * transfer}
        height={12}
        rx={6}
        fill={colors.teal}
      />
    </svg>
  );
}

function NetworkMobile({
  phase,
  phoneY,
  progress,
}: {
  phase: number;
  phoneY: number;
  progress: number;
}) {
  const activeStop = phase === 0 ? 0 : phase <= 2 ? 1 : 2;
  const activeY = phase === 0 ? 180 : phase <= 2 ? 355 : 530;
  const routeColor =
    phase === 0
      ? colors.blue
      : phase === 1 || phase === 3 || phase === 4
        ? colors.amber
        : colors.teal;
  return (
    <svg
      viewBox="0 0 520 690"
      role="img"
      aria-label="На мобільному екрані: телефон рухається вниз від Wi-Fi через вежу A до вежі B; маршрут веде до сервера."
      style={{ display: "block", width: "100%", height: "auto" }}
    >
      <Server x={380} y={55} />
      <text
        x={380}
        y={110}
        fill={colors.ink}
        textAnchor="middle"
        fontSize={18}
        fontWeight={800}
      >
        СЕРВЕР · СТАН 7F3A
      </text>
      <path
        d="M380 93V610"
        stroke={colors.line}
        strokeWidth={4}
        strokeDasharray="9 10"
      />
      <path
        d={`M380 ${activeY}V93`}
        stroke={routeColor}
        strokeWidth={7}
        strokeLinecap="round"
      />
      <line
        x1={120}
        y1={180}
        x2={120}
        y2={530}
        stroke={colors.line}
        strokeWidth={4}
      />
      <line
        x1={120}
        y1={180}
        x2={120}
        y2={phoneY}
        stroke={colors.blue}
        strokeWidth={5}
      />
      {[180, 355, 530].map((y, index) => (
        <g key={y}>
          <circle
            cx={120}
            cy={y}
            r={9}
            fill={index === activeStop ? colors.teal : colors.line}
          />
          <line
            x1={130}
            y1={y}
            x2={324}
            y2={y}
            stroke={index === activeStop ? routeColor : colors.line}
            strokeWidth={3}
            strokeDasharray="7 8"
          />
          {index === 0 ? (
            <Router
              x={380}
              y={y}
              color={
                index === activeStop
                  ? index === 0
                    ? colors.blue
                    : index === 1
                      ? colors.amber
                      : colors.teal
                  : colors.line
              }
            />
          ) : (
            <Tower
              x={380}
              y={y}
              color={index === activeStop ? routeColor : colors.line}
            />
          )}
          <text
            x={185}
            y={y - 18}
            fill={index === activeStop ? colors.teal : colors.muted}
            fontSize={17}
            fontWeight={800}
          >
            {index === 0 ? "WI‑FI" : `ВЕЖА ${index === 1 ? "A" : "B"}`}
          </text>
        </g>
      ))}
      <Phone x={120} y={phoneY} />
      <text x={92} y={620} fill={colors.muted} fontSize={14} fontWeight={800}>
        РУХ ТЕЛЕФОНА
      </text>
      <rect x={40} y={650} width={440} height={12} rx={6} fill={colors.line} />
      <rect
        x={40}
        y={650}
        width={440 * progress}
        height={12}
        rx={6}
        fill={colors.teal}
      />
    </svg>
  );
}

const paths = [
  { name: "Wi-Fi", ip: "192.0.2.10", cid: "0xA7" },
  { name: "Вежа A", ip: "198.51.100.24", cid: "0xB2" },
  { name: "Вежа B", ip: "203.0.113.7", cid: "0xC4" },
];

export function MigrationDiagram({ time }: { readonly time: number }) {
  const seconds = Number.isFinite(time) ? clamp(time, 0, 18) : 0;
  const progress = (from: number, to: number) =>
    ease((seconds - from) / (to - from));
  const phoneX =
    seconds < 4
      ? 220
      : seconds < 7
        ? 220 + 390 * progress(4, 7)
        : seconds < 8
          ? 610
          : seconds < 12
            ? 610 + 390 * progress(8, 12)
            : 1000;
  const transfer =
    seconds < 7
      ? 0.42 * progress(0, 7)
      : seconds < 8
        ? 0.42 + 0.04 * progress(7, 8)
        : seconds < 12
          ? 0.46 + 0.26 * progress(8, 12)
          : seconds < 15
            ? 0.72 + 0.05 * progress(12, 15)
            : 0.77 + 0.23 * progress(15, 18);
  const phase =
    seconds < 4
      ? 0
      : seconds < 7
        ? 1
        : seconds < 8
          ? 2
          : seconds < 12
            ? 3
            : seconds < 15
              ? 4
              : 5;
  const currentPath = phase === 0 ? 0 : phase <= 2 ? 1 : 2;
  const status =
    phase === 0
      ? "Сервер уже знає це з’єднання."
      : phase === 1 || phase === 3 || phase === 4
        ? "Сервер знайшов з’єднання за CID і перевіряє новий шлях."
        : "Новий шлях підтверджено. Той самий потік продовжує завантаження.";
  return (
    <section
      lang="uk"
      aria-label="Міграція QUIC-з’єднання між мережами"
      style={{
        ...baseText,
        color: colors.ink,
        background: colors.bg,
        padding: "clamp(10px, 2vw, 24px)",
        borderRadius: 18,
        width: "100%",
        boxSizing: "border-box",
      }}
    >
      <style>{`
        .he-migration-desktop { display: block; }
        .he-migration-mobile { display: none; }
        @media (max-width: 700px) {
          .he-migration-desktop { display: none; }
          .he-migration-mobile { display: block; }
        }
      `}</style>
      <div
        style={{
          background: colors.panel,
          border: `1px solid ${colors.line}`,
          borderRadius: 14,
          padding: "8px clamp(8px, 1.5vw, 18px)",
        }}
      >
        <div className="he-migration-desktop">
          <NetworkDesktop phase={phase} phoneX={phoneX} transfer={transfer} />
        </div>
        <div className="he-migration-mobile">
          <NetworkMobile
            phase={phase}
            phoneY={
              seconds < 4
                ? 180
                : seconds < 7
                  ? 180 + 175 * progress(4, 7)
                  : seconds < 8
                    ? 355
                    : seconds < 12
                      ? 355 + 175 * progress(8, 12)
                      : 530
            }
            progress={transfer}
          />
        </div>
      </div>

      <div
        style={{
          display: "grid",
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 195px), 1fr))",
          gap: 8,
          marginTop: 9,
        }}
      >
        {paths.map(({ name, ip, cid }, i) => (
          <div
            key={name}
            aria-current={i === currentPath ? "location" : undefined}
            style={{
              padding: "6px 10px",
              borderRadius: 6,
              background: i === currentPath ? "#153a35" : colors.panel,
              border: `1px solid ${i === currentPath ? colors.teal : colors.line}`,
              color: i <= currentPath ? colors.ink : colors.muted,
            }}
          >
            <b>{name}</b> <code>{i <= currentPath ? ip : "—"}</code>
            <div style={{ fontSize: 14, marginTop: 2 }}>
              {i <= currentPath
                ? `DCID ${cid} → з’єднання 7F3A`
                : "Очікує переходу"}
            </div>
          </div>
        ))}
      </div>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          flexWrap: "wrap",
          gap: 5,
          marginTop: 8,
          fontSize: 14,
        }}
      >
        <span>{status}</span>
        <b>Завантажено {Math.round(transfer * 100)}%</b>
      </div>
    </section>
  );
}
