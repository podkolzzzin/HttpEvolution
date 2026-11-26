const C = {
  bg: "#f7f2e7",
  ink: "#243b3a",
  muted: "#64716b",
  line: "#d9ddce",
  teal: "#167c74",
  blue: "#477fa5",
  red: "#d76655",
  yellow: "#e7bb4f",
  orange: "#dd9050",
  green: "#78a56a",
  purple: "#8b79a7",
  white: "#fffdf6",
};
const clamp = (n: number, a: number, b: number) => Math.max(a, Math.min(b, n));
const ease = (n: number) => {
  const p = clamp(n, 0, 1);
  return p * p * (3 - 2 * p);
};
const mix = (a: number, b: number, p: number) => a + (b - a) * ease(p);
export const TLS_EXCHANGE_DURATION = 72;
const beats = [
  {
    title: "Відкритий ключ сервера",
    detail: "Сервер надсилає сертифікат із відкритим ключем",
  },
  {
    title: "Секрет у скриньці",
    detail: "Браузер створює секрет і закриває його відкритим ключем",
  },
  {
    title: "Лише сервер відкриє скриньку",
    detail: "Сервер розшифровує отриманий секрет своїм приватним ключем",
  },
  {
    title: "Два напрямки — два ключі",
    detail:
      "Із секрету обидві сторони обчислюють ключ для запитів і ключ для відповідей",
  },
  {
    title: "Що станеться із записаною розмовою?",
    detail:
      "Якщо ключ сервера викрадуть пізніше, старий запис можна розшифрувати",
  },
  {
    title: "У кожного свій секретний колір",
    detail: "До спільної жовтої основи кожен додає свою приватну фарбу",
  },
  {
    title: "Відкрита суміш A",
    detail: "Браузер надсилає A серверу, зберігаючи приватну фарбу a",
  },
  {
    title: "Відкрита суміш B",
    detail: "Сервер надсилає B браузеру, зберігаючи приватну фарбу b",
  },
  {
    title: "В обох виходить той самий колір",
    detail: "Кожен додає свою приватну фарбу до отриманої суміші",
  },
  {
    title: "Сервер доводить свою особу",
    detail:
      "Сервер підписує цей обмін. Браузер перевіряє підпис ключем із сертифіката",
  },
  {
    title: "Finished підтверджує обмін",
    detail:
      "Спершу сервер, потім браузер підтверджують завершення рукостискання",
  },
  {
    title: "Захищений HTTP",
    detail:
      "Браузер просить сторінку. Сервер відповідає через те саме з’єднання",
  },
];
export function getTlsExchangeBeat(time: number): {
  title: string;
  detail: string;
} {
  const t = clamp(
    Number.isFinite(time) ? time : 0,
    0,
    TLS_EXCHANGE_DURATION - 0.001,
  );
  return beats[Math.floor(t / 6)];
}
function Lock({
  x,
  y,
  open = false,
  color = C.teal,
}: {
  x: number;
  y: number;
  open?: boolean;
  color?: string;
}) {
  return (
    <g
      transform={`translate(${x} ${y})`}
      stroke={color}
      strokeWidth="4"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path
        d={open ? "M-14 0v-12c0-18 29-19 29-2" : "M-14 0v-12c0-19 28-19 28 0V0"}
        fill="none"
      />
      <rect x="-23" y="0" width="46" height="35" rx="7" fill={C.white} />
      <circle cx="0" cy="14" r="3" fill={color} stroke="none" />
      <path d="M0 17v7" />
    </g>
  );
}
function Certificate({
  x,
  y,
  identity = false,
}: {
  x: number;
  y: number;
  identity?: boolean;
}) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <path
        d="M-49-55h72l26 26v83h-98z"
        fill={C.white}
        stroke={C.teal}
        strokeWidth="3"
      />
      <path
        d="M23-55v26h26M-33-35h40M-33-22h35"
        fill="none"
        stroke={C.teal}
        strokeWidth="3"
      />
      {identity ? (
        <path
          d="M-20 17l14 14 29-33"
          fill="none"
          stroke={C.teal}
          strokeWidth="5"
        />
      ) : (
        <Lock x={0} y={5} open />
      )}
    </g>
  );
}
function Box({ x, y, open = false }: { x: number; y: number; open?: boolean }) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <path
        d={open ? "M-55-22l11-25h89l10 25" : "M-55-22h110v62h-110z"}
        fill="#d8a46e"
        stroke="#775d4d"
        strokeWidth="3"
      />
      <path
        d="M-55-22h110v62h-110z"
        fill="#e5ba86"
        stroke="#775d4d"
        strokeWidth="3"
      />
      <path d="M-55-8h110" stroke="#ad7a55" strokeWidth="3" />
      <Lock x={0} y={-8} open={open} color="#775d4d" />
    </g>
  );
}
function Key({
  x,
  y,
  color,
  label,
}: {
  x: number;
  y: number;
  color: string;
  label?: string;
}) {
  return (
    <g
      transform={`translate(${x} ${y})`}
      stroke={color}
      strokeWidth="6"
      fill="none"
      strokeLinecap="round"
    >
      <circle cx="-26" r="13" />
      <path d="M-13 0h62m-13 0v11m-12-11v8" />
      {label && (
        <text
          x="10"
          y="38"
          textAnchor="middle"
          fill={color}
          stroke="none"
          fontSize="20"
          fontWeight="700"
        >
          {label}
        </text>
      )}
    </g>
  );
}
function Cup({ x, color, label }: { x: number; color: string; label: string }) {
  return (
    <g transform={`translate(${x} 250)`}>
      <ellipse cx="0" cy="70" rx="74" ry="12" fill="#d8d3c3" opacity=".55" />
      <path
        d="M-58-50h116l-11 105q-47 20-94 0z"
        fill={C.white}
        stroke={C.ink}
        strokeWidth="3"
      />
      <path d="M-52-6h104l-5 61q-47 20-94 0z" fill={color} opacity=".88" />
      <ellipse cy="-6" rx="52" ry="11" fill={color} />
      <ellipse
        cy="-50"
        rx="58"
        ry="12"
        fill={C.white}
        stroke={C.ink}
        strokeWidth="3"
      />
      <text
        y="102"
        textAnchor="middle"
        fill={C.ink}
        fontSize="22"
        fontWeight="750"
      >
        {label}
      </text>
    </g>
  );
}
function Drop({
  x,
  y,
  color,
  label,
}: {
  x: number;
  y: number;
  color: string;
  label: string;
}) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <path
        d="M0-26C-8-12-19-1-19 11a19 19 0 0 0 38 0C19-1 8-12 0-26z"
        fill={color}
      />
      <text
        y="17"
        textAnchor="middle"
        fill="white"
        fontSize="20"
        fontWeight="800"
      >
        {label}
      </text>
    </g>
  );
}
function Vial({
  x,
  color,
  label,
}: {
  x: number;
  color: string;
  label: string;
}) {
  return (
    <g transform={`translate(${x} 150)`}>
      <path
        d="M-19-31h38v15l9 13v34h-56V-3l9-13z"
        fill={C.white}
        stroke={color}
        strokeWidth="3"
      />
      <path d="M-26 3h52v26h-52z" fill={color} />
      <path d="M-21-31h42" stroke={color} strokeWidth="5" />
      <text
        y="22"
        textAnchor="middle"
        fill={C.white}
        fontSize="21"
        fontWeight="800"
      >
        {label}
      </text>
    </g>
  );
}
function Message({
  x,
  y,
  text,
  color,
}: {
  x: number;
  y: number;
  text: string;
  color: string;
}) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <rect
        x="-75"
        y="-25"
        width="150"
        height="50"
        rx="12"
        fill={C.white}
        stroke={color}
        strokeWidth="3"
      />
      <text
        y="8"
        textAnchor="middle"
        fill={color}
        fontSize="22"
        fontWeight="800"
      >
        {text}
      </text>
    </g>
  );
}
export function TlsKeyExchangeDiagram({ time }: { time: number }) {
  const t = clamp(
    Number.isFinite(time) ? time : 0,
    0,
    TLS_EXCHANGE_DURATION - 0.001,
  );
  const i = Math.floor(t / 6),
    p = (t % 6) / 6,
    b = beats[i];
  const travel = (from: number, to: number, start = 0.1, end = 0.85) =>
    mix(from, to, (p - start) / (end - start));
  return (
    <svg
      viewBox="0 0 900 480"
      role="img"
      aria-label={`Обмін ключами TLS. ${b.title}. ${b.detail}. Тривалість 72 секунди.`}
      style={{
        display: "block",
        width: "100%",
        height: "auto",
        background: C.bg,
        fontFamily: "Inter, system-ui, sans-serif",
      }}
    >
      <rect width="900" height="480" fill={C.bg} />
      <circle cx="88" cy="70" r="90" fill="#efe8d8" />
      <circle cx="825" cy="320" r="130" fill="#eee9da" />
      <text
        x="450"
        y="39"
        textAnchor="middle"
        fill={C.muted}
        fontSize="18"
        fontWeight="800"
        letterSpacing="2"
      >
        {i < 5
          ? "СТАРИЙ RSA-ОБМІН"
          : i < 9
            ? "ECDHE · АНАЛОГІЯ З ФАРБАМИ"
            : "TLS 1.3 · ЗАХИЩЕНИЙ ОБМІН"}
      </text>
      <path
        d="M190 100H710"
        stroke={C.line}
        strokeWidth="3"
        strokeDasharray="7 9"
      />
      <circle cx="190" cy="100" r="9" fill={C.teal} />
      <circle cx="710" cy="100" r="9" fill={C.blue} />
      <text
        x="190"
        y="78"
        textAnchor="middle"
        fill={C.ink}
        fontSize="25"
        fontWeight="800"
      >
        Браузер
      </text>
      <text
        x="710"
        y="78"
        textAnchor="middle"
        fill={C.ink}
        fontSize="25"
        fontWeight="800"
      >
        Сервер
      </text>
      {i === 0 && (
        <>
          <Certificate x={travel(710, 190)} y={205} />
          <Key x={710} y={305} color={C.blue} label="приватний RSA" />
        </>
      )}
      {i === 1 && (
        <>
          <Box x={190} y={250} open={p < 0.48} />
          <g
            transform={`translate(190 ${mix(174, 240, p / 0.52)})`}
            opacity={p < 0.56 ? 1 : 0}
          >
            <rect
              x="-25"
              y="-32"
              width="50"
              height="62"
              rx="3"
              fill={C.white}
              stroke={C.orange}
              strokeWidth="3"
            />
            <text
              y="7"
              textAnchor="middle"
              fill={C.orange}
              fontSize="20"
              fontWeight="800"
            >
              S
            </text>
          </g>
          <text
            x="190"
            y="340"
            textAnchor="middle"
            fill={C.ink}
            fontSize="21"
            fontWeight="700"
          >
            секрет
          </text>
          <Key x={710} y={240} color={C.blue} label="приватний RSA" />
        </>
      )}
      {i === 2 && (
        <>
          <Box x={travel(190, 710)} y={230} open={p > 0.88} />
          {p > 0.88 && (
            <g transform={`translate(710 ${mix(215, 175, (p - 0.88) / 0.1)})`}>
              <rect
                x="-25"
                y="-32"
                width="50"
                height="62"
                rx="3"
                fill={C.white}
                stroke={C.orange}
                strokeWidth="3"
              />
              <text
                y="7"
                textAnchor="middle"
                fill={C.orange}
                fontSize="20"
                fontWeight="800"
              >
                S
              </text>
            </g>
          )}
          <Key x={710} y={330} color={C.blue} label="приватний RSA" />
        </>
      )}
      {i === 3 && (
        <>
          <Key x={190} y={200} color={C.teal} label="браузер → сервер" />
          <Key x={710} y={200} color={C.teal} label="браузер → сервер" />
          <Key x={190} y={290} color={C.purple} label="сервер → браузер" />
          <Key x={710} y={290} color={C.purple} label="сервер → браузер" />
        </>
      )}
      {i === 4 && (
        <>
          <Box x={450} y={235} open={p > 0.62} />
          <text
            x="450"
            y="325"
            textAnchor="middle"
            fill={C.ink}
            fontSize="21"
            fontWeight="700"
          >
            записаний обмін
          </text>
          <text
            x="710"
            y="145"
            textAnchor="middle"
            fill={C.red}
            fontSize="20"
            fontWeight="700"
          >
            викрадений ключ
          </text>
          <g transform={`translate(${mix(710, 530, p / 0.65) - 710} 0)`}>
            <Key x={710} y={190} color={C.red} />
          </g>
        </>
      )}
      {i >= 5 && i <= 8 && (
        <>
          <Cup
            x={190}
            color={
              i === 5 && p < 0.55
                ? C.yellow
                : i === 8 && p > 0.55
                  ? C.purple
                  : i === 8
                    ? C.green
                    : C.orange
            }
            label={
              i === 8 && p > 0.55
                ? "спільний S"
                : i === 8
                  ? "отримано B"
                  : i === 5 && p < 0.55
                    ? "спільна основа"
                    : "відкритий A"
            }
          />
          <Cup
            x={710}
            color={
              i === 5 && p < 0.55
                ? C.yellow
                : i === 8 && p > 0.55
                  ? C.purple
                  : i === 8
                    ? C.orange
                    : C.green
            }
            label={
              i === 8 && p > 0.55
                ? "спільний S"
                : i === 8
                  ? "отримано A"
                  : i === 5 && p < 0.55
                    ? "спільна основа"
                    : "відкритий B"
            }
          />
          {i !== 8 && (
            <>
              <Drop
                x={i === 5 ? mix(108, 190, p / 0.55) : 108}
                y={i === 5 ? mix(133, 220, p / 0.55) : 174}
                color={C.red}
                label="a"
              />
              <Drop
                x={i === 5 ? mix(792, 710, p / 0.55) : 792}
                y={i === 5 ? mix(133, 220, p / 0.55) : 174}
                color={C.blue}
                label="b"
              />
              <text
                x="108"
                y="230"
                textAnchor="middle"
                fill={C.red}
                fontSize="19"
                fontWeight="700"
              >
                приватна
              </text>
              <text
                x="792"
                y="230"
                textAnchor="middle"
                fill={C.blue}
                fontSize="19"
                fontWeight="700"
              >
                приватна
              </text>
            </>
          )}
          {i === 6 && <Vial x={travel(190, 710)} label="A" color={C.orange} />}
          {i === 7 && <Vial x={travel(710, 190)} label="B" color={C.green} />}
          {i === 8 && (
            <>
              <Drop
                x={mix(108, 190, p / 0.6)}
                y={mix(174, 220, p / 0.6)}
                color={C.red}
                label="a"
              />
              <Drop
                x={mix(792, 710, p / 0.6)}
                y={mix(174, 220, p / 0.6)}
                color={C.blue}
                label="b"
              />
            </>
          )}
        </>
      )}
      {i === 9 && (
        <>
          <Certificate identity x={travel(710, 190)} y={175} />
          <text
            x={travel(710, 190)}
            y="255"
            textAnchor="middle"
            fill={C.teal}
            fontSize="20"
            fontWeight="700"
          >
            сертифікат + підпис
          </text>
          <g transform="translate(710 300)">
            <path
              d="M-75-48h150v96h-150z"
              fill={C.white}
              stroke={C.blue}
              strokeWidth="3"
            />
            <path d="M-52-20h104M-52 0h70" stroke={C.line} strokeWidth="5" />
            <path
              d="M16 24l15 10 28-35"
              fill="none"
              stroke={C.blue}
              strokeWidth="6"
            />
          </g>
          <text
            x="710"
            y="365"
            textAnchor="middle"
            fill={C.blue}
            fontSize="21"
            fontWeight="700"
          >
            підпис обміну
          </text>
          {p > 0.75 && (
            <text
              x="190"
              y="330"
              textAnchor="middle"
              fill={C.teal}
              fontSize="20"
              fontWeight="700"
            >
              домен · довіра · підпис ✓
            </text>
          )}
        </>
      )}
      {i === 10 && (
        <>
          <Lock x={190} y={215} />
          <Lock x={710} y={215} color={C.blue} />
          <Message
            x={
              p < 0.5 ? travel(710, 190, 0, 0.46) : travel(190, 710, 0.5, 0.96)
            }
            y={205}
            text="Finished"
            color={p < 0.5 ? C.blue : C.teal}
          />
        </>
      )}
      {i === 11 && (
        <>
          <Lock x={190} y={215} />
          <Lock x={710} y={215} color={C.blue} />
          <Message
            x={
              p < 0.5 ? travel(190, 710, 0, 0.46) : travel(710, 190, 0.5, 0.96)
            }
            y={205}
            text={p < 0.5 ? "GET / 🔒" : "відповідь 🔒"}
            color={p < 0.5 ? C.teal : C.blue}
          />
        </>
      )}
      <path d="M45 375H855" stroke={C.line} strokeWidth="2" />
      <text
        x="450"
        y="410"
        textAnchor="middle"
        fill={C.ink}
        fontSize="25"
        fontWeight="800"
      >
        {b.title}
      </text>
      <text
        x="450"
        y="443"
        textAnchor="middle"
        fill={C.muted}
        fontSize="20"
        fontWeight="600"
      >
        {b.detail}
      </text>
    </svg>
  );
}
