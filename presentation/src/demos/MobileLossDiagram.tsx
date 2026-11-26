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
  red: "#ff667d",
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
  lineHeight: 1.45,
};

function Packet({ label, color }: { label: string; color: string }) {
  return (
    <span
      style={{
        display: "inline-flex",
        alignItems: "center",
        justifyContent: "center",
        minWidth: 42,
        minHeight: 34,
        padding: "0 9px",
        boxSizing: "border-box",
        borderRadius: 9,
        border: `2px solid ${color}`,
        background: `${color}24`,
        color: colors.ink,
        fontSize: 15,
        fontWeight: 900,
        whiteSpace: "nowrap",
      }}
    >
      {label}
    </span>
  );
}

function Lane({ time, quic }: { time: number; quic: boolean }) {
  const lost = time >= 3.2;
  const arrived = time >= 5.2;
  const repaired = time >= 11.5;
  const delivered = time >= 13.2;
  const travel = (start: number, end: number) =>
    `${10 + ease((time - start) / (end - start)) * 68}%`;
  const status = quic
    ? delivered
      ? "A відновлено; застосунок отримав A та B."
      : arrived
        ? "B вже в застосунку. Втрата A затримує лише дані у потоці A."
        : lost
          ? "A втрачено; B рухається незалежним потоком."
          : "Потік B рухається незалежно від затримки у потоці A."
    : delivered
      ? "Після відновлення A TCP віддає застосунку A та накопичений B."
      : repaired
        ? "A повторно передається; TCP зберігає порядок і поки утримує B."
        : arrived
          ? "B вже фізично у приймачі, але TCP тримає його в буфері до відновлення A."
          : "TCP зберігає спільний порядок байтів.";
  const stateColor = quic
    ? colors.teal
    : arrived && !delivered
      ? colors.amber
      : colors.muted;
  const bufferText = quic
    ? repaired && !delivered
      ? "A ↻ · відновлюється"
      : lost && !delivered && !arrived
        ? "A · очікує повтору"
        : delivered
          ? "порожньо · A відновлено"
          : "порожньо"
    : delivered
      ? "порожньо · порядок відновлено"
      : repaired
        ? "A відновлено · B чекає на порядок"
        : arrived
          ? "B · чекає на A"
          : "пакетів ще немає";
  const appText = quic
    ? delivered
      ? "A + B · доставлено"
      : arrived
        ? "B · доставлено"
        : "очікує"
    : delivered
      ? "A + B · доставлено"
      : "чекає на A";

  return (
    <article
      aria-label={
        quic
          ? "HTTP/3: втрата в одному QUIC-потоці"
          : "HTTP/2: втрата у спільному TCP-потоці"
      }
      style={{
        background: colors.panel,
        border: `1px solid ${colors.line}`,
        borderRadius: 15,
        padding: "clamp(11px, 1.5vw, 18px)",
        minWidth: 0,
      }}
    >
      <h3
        style={{
          ...baseText,
          margin: "0 0 10px",
          color: colors.ink,
          fontSize: 18,
          fontWeight: 850,
        }}
      >
        {quic
          ? "HTTP/3 · незалежні потоки QUIC"
          : "HTTP/2 · спільний порядок TCP"}
      </h3>
      <div
        style={{
          display: "grid",
          gridTemplateColumns:
            "minmax(64px, 92px) minmax(0, 1fr) minmax(72px, 104px)",
          alignItems: "center",
          gap: 8,
        }}
      >
        <div
          style={{
            textAlign: "center",
            color: colors.muted,
            fontSize: 13,
            fontWeight: 750,
          }}
        >
          ВІДПРАВНИК
        </div>
        <div
          style={{ position: "relative", height: 68, minWidth: 0 }}
          aria-hidden="true"
        >
          <div
            style={{
              position: "absolute",
              top: 45,
              left: 0,
              right: 0,
              height: 3,
              background: colors.line,
            }}
          />
          <span
            style={{
              position: "absolute",
              top: 39,
              left: 0,
              width: 14,
              height: 14,
              borderRadius: 8,
              background: colors.blue,
              outline: `3px solid ${colors.bg}`,
            }}
          />
          <span
            style={{
              position: "absolute",
              top: 39,
              right: 0,
              width: 14,
              height: 14,
              borderRadius: 8,
              background: colors.teal,
              outline: `3px solid ${colors.bg}`,
            }}
          />
          {time < 3.2 && (
            <div
              style={{
                position: "absolute",
                top: 10,
                left: `calc(${travel(1.4, 3.2)} - 22px)`,
              }}
            >
              <Packet label="A" color={colors.blue} />
            </div>
          )}
          {lost && !repaired && (
            <div
              style={{
                position: "absolute",
                top: 0,
                left: "78%",
                color: colors.red,
                fontSize: 13,
                fontWeight: 900,
                whiteSpace: "nowrap",
              }}
              aria-label="A втрачено"
            >
              A × ВТРАЧЕНО
            </div>
          )}
          {time >= 2.1 && time < 5.2 && (
            <div
              style={{
                position: "absolute",
                top: 32,
                left: `calc(${travel(2.1, 5.2)} - 22px)`,
              }}
            >
              <Packet label="B" color={colors.teal} />
            </div>
          )}
          {repaired && time < 13.2 && (
            <div
              style={{
                position: "absolute",
                top: 32,
                left: `calc(${travel(11.5, 13.2)} - 28px)`,
              }}
            >
              <Packet label="A ↻" color={colors.amber} />
            </div>
          )}
        </div>
        <div
          style={{
            textAlign: "center",
            color: colors.muted,
            fontSize: 13,
            fontWeight: 750,
          }}
        >
          ПРИЙМАЧ
        </div>
      </div>
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(2, minmax(0, 1fr))",
          gap: 8,
          marginTop: 3,
        }}
      >
        <div
          style={{
            minWidth: 0,
            padding: "9px 10px",
            borderRadius: 10,
            background: "#091423",
            border: `1px solid ${arrived && !delivered && !quic ? colors.amber : colors.line}`,
          }}
        >
          <div style={{ color: colors.muted, fontSize: 12, fontWeight: 750 }}>
            БУФЕР ПРИЙМАЧА
          </div>
          <div
            style={{
              marginTop: 3,
              color: arrived && !delivered && !quic ? colors.amber : colors.ink,
              fontSize: 14,
              fontWeight: 800,
            }}
          >
            {bufferText}
          </div>
        </div>
        <div
          style={{
            minWidth: 0,
            padding: "9px 10px",
            borderRadius: 10,
            background: "#091423",
            border: `1px solid ${(quic && arrived) || (!quic && delivered) ? colors.teal : colors.line}`,
          }}
        >
          <div style={{ color: colors.muted, fontSize: 12, fontWeight: 750 }}>
            ЗАСТОСУНОК
          </div>
          <div
            style={{
              marginTop: 3,
              color:
                (quic && arrived) || (!quic && delivered)
                  ? colors.teal
                  : colors.muted,
              fontSize: 14,
              fontWeight: 800,
            }}
          >
            {appText}
          </div>
        </div>
      </div>
      <p
        role="status"
        style={{
          ...baseText,
          minHeight: 44,
          margin: "9px 0 0",
          color: stateColor,
          fontWeight: 700,
        }}
      >
        {status}
      </p>
    </article>
  );
}

export function MobileLossDiagram({ time }: { readonly time: number }) {
  const seconds = Number.isFinite(time) ? clamp(time, 0, 16) : 0;
  return (
    <section
      lang="uk"
      aria-label="Втрата фрагмента даних у HTTP/2 і HTTP/3"
      style={{
        ...baseText,
        color: colors.ink,
        background: colors.bg,
        padding: "clamp(10px, 2vw, 24px)",
        borderRadius: 18,
        boxSizing: "border-box",
        width: "100%",
        display: "grid",
        gap: 10,
      }}
    >
      <Lane time={seconds} quic={false} />
      <Lane time={seconds} quic />
    </section>
  );
}
