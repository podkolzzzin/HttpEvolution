import "./ProtocolConsole.css";

export const PROTOCOL_CONSOLE_DURATION = 25;

type Milestone = {
    version: string;
    year: string;
    terms: string;
    at: number;
};

const http: Milestone[] = [
    { version: "HTTP/0.9", year: "1991", terms: "HTML · GET", at: 0 },
    { version: "HTTP/1.0", year: "1996", terms: "Заголовки · статуси", at: 5 },
    { version: "HTTP/1.1", year: "1997", terms: "Постійні з’єднання", at: 7.1 },
    { version: "HTTP/2", year: "2015", terms: "Потоки · HPACK", at: 17.6 },
    { version: "HTTP/3", year: "2022", terms: "QUIC · QPACK", at: 23 },
];

const tls: Milestone[] = [
    { version: "SSL 2.0", year: "1995", terms: "RC4", at: 2.9 },
    { version: "SSL 3.0", year: "1996", terms: "3DES", at: 6 },
    { version: "TLS 1.0", year: "1999", terms: "CBC", at: 9.2 },
    { version: "TLS 1.1", year: "2006", terms: "Явний IV", at: 11.3 },
    { version: "TLS 1.2", year: "2008", terms: "AES-GCM", at: 13.4 },
    { version: "TLS 1.3", year: "2018", terms: "ChaCha20-Poly1305", at: 20 },
];

const chronology = [...http, ...tls].sort((a, b) => a.at - b.at);
const years = [...new Set(chronology.map((item) => item.year))];

function Track({
    title,
    items,
    time,
    tone,
}: {
    title: string;
    items: Milestone[];
    time: number;
    tone: "http" | "tls";
}) {
    return (
        <section className={`pc-track pc-${tone}`} aria-label={title}>
            <div className="pc-track-name">{title}</div>
            <div className="pc-rail">
                {items.map((item) => (
                    <div
                        className={`pc-stop${time >= item.at ? " is-on" : ""}`}
                        key={item.version}
                        style={{ gridColumn: years.indexOf(item.year) + 1 }}
                    >
                        <span className="pc-terms">{item.terms}</span>
                        <span className="pc-year">{item.year}</span>
                        <span className="pc-socket">
                            <span className="pc-bulb" />
                        </span>
                        <span className="pc-version">{item.version}</span>
                    </div>
                ))}
            </div>
        </section>
    );
}

/** Time is supplied in seconds, so seeking yields the same state. */
export function ProtocolConsole({ time }: { time: number }) {
    const t = Number.isFinite(time)
        ? Math.max(0, Math.min(PROTOCOL_CONSOLE_DURATION, time))
        : 0;
    const current = chronology.reduce(
        (latest, item) => (t >= item.at ? item : latest),
        chronology[0],
    );

    return (
        <div
            className="protocol-console"
            aria-label="Хронологія розвитку HTTP та SSL і TLS"
        >
            <div className="pc-shell">
                <div className="pc-topline">
                    <h2>
                        {current.version}{" "}
                        <span className="pc-current-term">{current.terms}</span>
                    </h2>
                    <div className="pc-counter">
                        <small>РІК</small>
                        <strong>{current.year}</strong>
                    </div>
                </div>
                <div className="pc-panel">
                    <div className="pc-screw pc-tl" />
                    <div className="pc-screw pc-tr" />
                    <div className="pc-screw pc-bl" />
                    <div className="pc-screw pc-br" />
                    <Track title="HTTP" items={http} time={t} tone="http" />
                    <div className="pc-divider" />
                    <Track title="SSL / TLS" items={tls} time={t} tone="tls" />
                </div>
                <div className="pc-progress">
                    <i
                        style={{
                            width: `${(t / PROTOCOL_CONSOLE_DURATION) * 100}%`,
                        }}
                    />
                </div>
            </div>
        </div>
    );
}
