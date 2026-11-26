import { useId, useState } from "react";
import type { ReactNode } from "react";

type Mode = "https" | "ech" | "http";
type Field = "host" | "sni";

const modes: readonly [Mode, string][] = [
    ["https", "HTTPS · домен відкритий"],
    ["ech", "HTTPS + ECH · приховати домен"],
    ["http", "HTTP · без шифрування"],
];

const request = "GET /orders HTTP/1.1\nHost: example.com";

function describe(field: Field, mode: Mode) {
    if (field === "sni") {
        return mode === "ech"
            ? [
                  "ECH · захист імені",
                  "Відкритий ключ ECH із конфігурації, зазвичай через DNS → внутрішній ClientHello під шифруванням",
              ]
            : [
                  "Без ECH · відкритий SNI",
                  "Спостерігач → example.com із ClientHello · шлях і Host усередині TLS",
              ];
    }

    return mode === "http"
        ? [
              "HTTP без шифрування",
              "Відкриті дані → Host: example.com і GET /orders",
          ]
        : [
              "Host усередині зашифрованого HTTP",
              "SNI → вибір сертифіката під час handshake · Host → вибір сайту після розшифрування HTTP",
          ];
}

function HostArrow() {
    return (
        <svg
            className="host-arrow"
            viewBox="0 0 26 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.5"
            aria-hidden="true"
        >
            <path d="M1 12h22m-7-7 7 7-7 7" />
        </svg>
    );
}

function Sealed({ children }: { children: ReactNode }) {
    return (
        <span className="host-lock">
            <svg
                viewBox="0 0 24 28"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.5"
                aria-hidden="true"
            >
                <rect x="3" y="12" width="18" height="13" rx="2" />
                <path d="M7 12V7a5 5 0 0 1 10 0v5m-5 5v4" />
            </svg>
            {children}
        </span>
    );
}

function Endpoint({
    title,
    code,
    note,
}: {
    title: string;
    code: string;
    note: string;
}) {
    return (
        <div className="host-endpoint">
            <strong>{title}</strong>
            <code>{code}</code>
            <p>{note}</p>
        </div>
    );
}

function Flow({
    browser,
    network,
    server,
}: {
    browser: ReactNode;
    network: ReactNode;
    server: ReactNode;
}) {
    return (
        <div className="host-flow">
            {browser}
            <HostArrow />
            {network}
            <HostArrow />
            {server}
        </div>
    );
}

type ObserverProps = {
    sealed: boolean;
    selected: boolean;
    detailId: string;
    onSelect: () => void;
    children: ReactNode;
};

function Observer({
    sealed,
    selected,
    detailId,
    onSelect,
    children,
}: ObserverProps) {
    return (
        <button
            type="button"
            className={sealed ? "host-observer host-sealed" : "host-observer"}
            aria-pressed={selected}
            aria-controls={detailId}
            onClick={onSelect}
        >
            <span className="host-observer-label">
                У мережі · що читає провайдер / власник Wi-Fi
            </span>
            {children}
        </button>
    );
}

export function HostPrivacyDemo() {
    const [mode, setMode] = useState<Mode>("https");
    const [selected, select] = useState<Field>("sni");
    const detailId = useId();
    const tls = mode !== "http";
    const ech = mode === "ech";
    const [title, body] = describe(selected, mode);

    function changeMode(nextMode: Mode) {
        setMode(nextMode);
        select(nextMode === "http" ? "host" : "sni");
    }

    return (
        <div className="host-privacy">
            <h2>Домен на двох рівнях</h2>
            <p className="host-intro">
                <strong>SNI</strong> у привітанні TLS · <strong>Host</strong> у
                HTTP-запиті
            </p>
            <div className="host-address">
                <span>Відкриваємо в браузері</span>
                <code>
                    {tls ? "https" : "http"}://<mark>example.com</mark>/orders
                </code>
            </div>
            <div className="traffic-controls">
                <div
                    className="switches"
                    role="group"
                    aria-label="Порівняти захист домену"
                >
                    {modes.map(([option, label]) => (
                        <button
                            key={option}
                            type="button"
                            aria-pressed={mode === option}
                            onClick={() => changeMode(option)}
                        >
                            {label}
                        </button>
                    ))}
                </div>
            </div>
            <p className="host-ech">
                <strong>ECH · Encrypted Client Hello</strong> · TLS 1.3 ·
                шифрування внутрішнього привітання й справжнього імені сайту
            </p>
            <div>
                {tls && (
                    <section className="host-phase">
                        <h3>1 · Ім’я сайту в TLS</h3>
                        <p>
                            ClientHello → SNI (Server Name Indication) →
                            сертифікат сайту
                        </p>
                        <Flow
                            browser={
                                <Endpoint
                                    title="Браузер · привітання"
                                    code={"ClientHello\nSNI: example.com"}
                                    note={
                                        ech
                                            ? "Внутрішній ClientHello → ECH"
                                            : "Відкритий ClientHello"
                                    }
                                />
                            }
                            network={
                                <Observer
                                    sealed={ech}
                                    selected={selected === "sni"}
                                    detailId={detailId}
                                    onSelect={() => select("sni")}
                                >
                                    {ech ? (
                                        <>
                                            <Sealed>
                                                Ім’я сайту приховане
                                            </Sealed>
                                            <b>
                                                Внутрішній ClientHello
                                                зашифрований
                                            </b>
                                            <small>
                                                Відкритим лишається зовнішнє
                                                ім’я:
                                                <br />
                                                <code>public.example</code>
                                                Спільне ім’я провайдера
                                            </small>
                                        </>
                                    ) : (
                                        <>
                                            <small>
                                                ClientHello → поле SNI
                                            </small>
                                            <code>example.com</code>
                                            <b>Відкрите ім’я</b>
                                            <small>Доступне спостерігачу</small>
                                        </>
                                    )}
                                </Observer>
                            }
                            server={
                                <Endpoint
                                    title="Сервер · вибір сертифіката"
                                    code="example.com"
                                    note={
                                        ech
                                            ? "Розшифрування ключем ECH"
                                            : "Ім’я сайту із SNI"
                                    }
                                />
                            }
                        />
                    </section>
                )}
                <section className="host-phase">
                    <h3>
                        {tls ? "2 · Запит /orders" : "HTTP · відкритий запит"}
                    </h3>
                    <p>
                        {tls
                            ? "Ключі сеансу → шифрування всього HTTP-запиту"
                            : "Без TLS · відкриті заголовки й тіло"}
                    </p>
                    <Flow
                        browser={
                            <Endpoint
                                title="Браузер · HTTP-запит"
                                code={request}
                                note={
                                    tls
                                        ? "Шифрування ключами сеансу"
                                        : "Текст у TCP"
                                }
                            />
                        }
                        network={
                            <Observer
                                sealed={tls}
                                selected={selected === "host"}
                                detailId={detailId}
                                onSelect={() => select("host")}
                            >
                                {tls ? (
                                    <>
                                        <Sealed>HTTP-запит прихований</Sealed>
                                        <b>Шлях і Host під шифруванням</b>
                                        <small>У мережі · шифротекст</small>
                                    </>
                                ) : (
                                    <>
                                        <code>Host: example.com</code>
                                        <b>Домен і шлях /orders</b>
                                        <small>Доступний увесь запит</small>
                                    </>
                                )}
                            </Observer>
                        }
                        server={
                            <Endpoint
                                title="Сервер · запит HTTP"
                                code={request}
                                note={
                                    tls
                                        ? "Після розшифрування TLS"
                                        : "Відкритий текст"
                                }
                            />
                        }
                    />
                </section>
            </div>
            <div
                id={detailId}
                className="traffic-detail"
                role="status"
                aria-atomic="true"
            >
                <h2>{title}</h2>
                <p>{body}</p>
            </div>
            <p className="host-history">
                <strong>Захист імені</strong>{" "}
                <a
                    href="https://blog.cloudflare.com/encrypted-sni/"
                    target="_blank"
                    rel="noopener"
                >
                    2018 — експериментальний ESNI (Encrypted SNI)
                </a>
                ·{" "}
                <a
                    href="https://blog.cloudflare.com/announcing-encrypted-client-hello/"
                    target="_blank"
                    rel="noopener"
                >
                    2023 — впровадження ECH у Cloudflare
                </a>
                ·{" "}
                <a
                    href="https://www.rfc-editor.org/rfc/rfc9849.html"
                    target="_blank"
                    rel="noopener"
                >
                    березень 2026 — стандарт ECH
                </a>
                · підтримка ECH обома сторонами
            </p>
            <p className="traffic-boundaries">
                Метадані навіть з ECH · IP, розміри, час · ризик відкритого DNS.
                1997 · Host.{" "}
                <a
                    href="https://www.rfc-editor.org/rfc/rfc3546.html#section-3.1"
                    target="_blank"
                    rel="noopener"
                >
                    2003 · стандарт SNI
                </a>
            </p>
        </div>
    );
}
