import { useEffect, useId, useLayoutEffect, useRef, useState } from "react";
import { Choices, Steps } from "./ProtocolControls";

const timelineTracks = [
    {
        id: "http",
        title: "Еволюція HTTP",
        events: [
            {
                year: "1991",
                name: "HTTP/0.9",
                slide: "http09",
                points: ["Один GET на TCP-з’єднання", "HTML без заголовків"],
            },
            {
                year: "1996",
                name: "HTTP/1.0",
                slide: "http10-status",
                points: [
                    "Коди стану й типи вмісту",
                    "Стискання тіла відповіді (gzip)",
                ],
            },
            {
                year: "1997",
                name: "HTTP/1.1",
                slide: "http11-arrives",
                points: [
                    "Кілька запитів через один TCP-сокет",
                    "Host і передавання частинами",
                ],
            },
            {
                year: "2011",
                name: "WebSocket",
                slide: "websocket-upgrade",
                points: [
                    "Перехід з HTTP/1.1 через Upgrade",
                    "Двосторонній обмін повідомленнями",
                ],
            },
            {
                year: "2015",
                name: "HTTP/2",
                slide: "http2-frames",
                points: [
                    "Паралельні потоки в одному TCP-з’єднанні",
                    "HPACK стискає заголовки",
                ],
            },
            {
                year: "2022",
                name: "HTTP/3",
                slide: "http3-arrives",
                points: [
                    "Незалежні потоки QUIC поверх UDP",
                    "QPACK стискає заголовки",
                ],
            },
        ],
    },
    {
        id: "encryption",
        title: "Еволюція шифрування",
        events: [
            {
                year: "1995",
                name: "SSL 2.0",
                slide: "ssl-versions",
                points: ["Перший публічний SSL", "HTTPS у браузері Netscape"],
            },
            {
                year: "1996",
                name: "SSL 3.0",
                slide: "ssl3-handshake",
                points: [
                    "Сертифікат засвідчує сервер",
                    "Finished перевіряє рукостискання",
                ],
            },
            {
                year: "1999",
                name: "TLS 1.0",
                slide: "tls10",
                points: [
                    "IETF стандартизує наступника SSL",
                    "HMAC захищає цілісність записів",
                ],
            },
            {
                year: "2002",
                name: "AES у TLS",
                slide: "tls-aes",
                points: [
                    "Нові набори шифрів з AES",
                    "Клієнт і сервер узгоджують набір",
                ],
            },
            {
                year: "2018",
                name: "TLS 1.3",
                slide: "tls13-arrives",
                points: [
                    "Звичайне рукостискання за 1 RTT",
                    "AEAD шифрує й перевіряє цілісність",
                ],
            },
            {
                year: "2021",
                name: "TLS у QUIC",
                slide: "quic-protection",
                points: ["TLS 1.3 узгоджує ключі", "QUIC захищає ними пакети"],
            },
        ],
    },
];

function fitTimelineSlide(slide: HTMLElement) {
    const scaleProperty = "--slide-scale";
    slide.style.removeProperty(scaleProperty);

    const available = Number.parseFloat(getComputedStyle(slide).minHeight);
    if (available <= 0 || slide.getBoundingClientRect().height <= available) {
        return;
    }

    // A wider layout at smaller scales can wrap fewer lines.
    let minimum = 1;
    let maximum = 99;
    while (minimum < maximum) {
        const percent = Math.ceil((minimum + maximum) / 2);
        slide.style.setProperty(scaleProperty, String(percent / 100));
        if (slide.getBoundingClientRect().height <= available) {
            minimum = percent;
        } else {
            maximum = percent - 1;
        }
    }
    slide.style.setProperty(scaleProperty, String(minimum / 100));
}

export function Timeline({ active }: { active: boolean }) {
    const tracks = useRef<HTMLDivElement>(null);

    useLayoutEffect(() => {
        const slide = tracks.current?.closest<HTMLElement>("#start");
        const deck = slide?.parentElement;
        if (!active || !slide || !deck?.matches(".deck")) return;

        fitTimelineSlide(slide);
        const observer = new ResizeObserver(() => fitTimelineSlide(slide));
        observer.observe(deck);
        return () => {
            observer.disconnect();
            slide.style.removeProperty("--slide-scale");
        };
    }, [active]);

    return (
        <div className="protocol-timelines" ref={tracks}>
            {timelineTracks.map(({ id, title, events }) => (
                <section
                    className={`timeline-track track-${id}`}
                    key={id}
                    aria-labelledby={`track-${id}`}
                >
                    <h2 id={`track-${id}`}>{title}</h2>
                    <ol className="http-timeline">
                        {events.map(({ year, name, slide, points }) => (
                            <li key={slide}>
                                <a href={`#${slide}`}>
                                    <time dateTime={year}>{year}</time>
                                    <span>{name}</span>
                                </a>
                                <ul>
                                    {points.map((point) => (
                                        <li key={point}>{point}</li>
                                    ))}
                                </ul>
                            </li>
                        ))}
                    </ol>
                </section>
            ))}
        </div>
    );
}

const resources = [
    ['<link rel="stylesheet" href="/site.css">', "/site.css", "Стилі"],
    ['<script src="/cart.js"></script>', "/cart.js", "Сценарій"],
    ['<img src="/logo.gif">', "/logo.gif", "Логотип"],
    ['<img src="/book.jpg">', "/book.jpg", "Обкладинка"],
    ['<img src="/map.gif">', "/map.gif", "Карта"],
];
export function ResourceTree() {
    const [selected, select] = useState(0);
    return (
        <div className="protocol-panel resource-tree">
            <div className="protocol-head">
                <strong>GET /index.html</strong>
                <span>200 OK · text/html</span>
            </div>
            <div className="resource-graph">
                <div className="html-file">
                    <h2>/index.html</h2>
                    <code>&lt;html&gt;</code>
                    {resources.map(([tag, path], i) => (
                        <code
                            className={selected === i ? "active-tag" : ""}
                            key={path}
                        >
                            {tag}
                        </code>
                    ))}
                    <code>&lt;/html&gt;</code>
                </div>
                <div className="resource-branches" aria-label="Запити з HTML">
                    {resources.map(([, path, label], i) => (
                        <button
                            key={path}
                            type="button"
                            aria-pressed={selected === i}
                            onClick={() => select(i)}
                        >
                            <code>GET {path}</code>
                            <small>{label}</small>
                        </button>
                    ))}
                </div>
            </div>
            <p className="protocol-caption">
                Порожній кеш · 1 документ + 5 ресурсів · 6 запитів
            </p>
        </div>
    );
}

function SslMacTooltip() {
    const tooltipId = useId();
    const [visible, setVisible] = useState(false);

    useEffect(() => {
        if (!visible) return;

        function dismiss(event: KeyboardEvent) {
            if (event.key === "Escape") setVisible(false);
        }

        document.addEventListener("keydown", dismiss);
        return () => document.removeEventListener("keydown", dismiss);
    }, [visible]);

    return (
        <span
            onPointerLeave={(event) => {
                if (event.pointerType === "mouse" && !event.currentTarget.matches(":focus-within")) {
                    setVisible(false);
                }
            }}
        >
            <button
                type="button"
                className="ssl-mac-term"
                aria-describedby={tooltipId}
                onPointerEnter={(event) => {
                    if (event.pointerType === "mouse") setVisible(true);
                }}
                onFocus={() => setVisible(true)}
                onBlur={() => setVisible(false)}
                onClick={(event) => {
                    event.currentTarget.focus({ preventScroll: true });
                    setVisible(true);
                }}
            >
                MAC
            </button>
            <span id={tooltipId} className="ssl-mac-tooltip" role="tooltip" hidden={!visible}>
                <strong>Message Authentication Code</strong>
                <span>Код, який обчислюють із даних і секретного ключа, щоб виявити підміну.</span>
            </span>
        </span>
    );
}

export function SslDesign() {
    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>HTTP → SSL → TCP</strong>
                <span>Браузер ↔ сервер</span>
            </div>
            <ol className="protocol-ledger">
                <li>
                    <b>Перевірка сервера</b>
                    <span>
                        Ім’я сайту + сертифікат + довірений підпис
                        <small className="ssl-certificate-note">
                            Першу версію стандарту сертифікатів X.509 затвердили ще у 1988 році.
                        </small>
                    </span>
                </li>
                <li>
                    <b>Домовленість</b>
                    <span>
                        ClientHello / ServerHello → спільний набір алгоритмів
                    </span>
                </li>
                <li>
                    <b>Ключі сеансу</b>
                    <span className="ssl-mac-line">
                        Спільний секрет → окремі ключі шифрування й{" "}
                        <SslMacTooltip /> для обох напрямків
                    </span>
                </li>
                <li>
                    <b>Перевірка переговорів</b>
                    <span>Finished → підтвердження історії handshake</span>
                </li>
                <li>
                    <b>Передача HTTP</b>
                    <span className="ssl-mac-line">
                        Фрагмент + <SslMacTooltip /> → симетричне шифрування запису
                    </span>
                </li>
            </ol>
        </div>
    );
}

const responseEncoder = new TextEncoder();
const messages = [
    { path: "/a", chunks: ["O", "NE"] },
    { path: "/b", chunks: ["T", "WO"] },
    { path: "/c", chunks: ["TH", "REE"] },
].map(({ path, chunks }) => {
    const body = chunks.join("");
    const length = responseEncoder.encode(body).length;
    const chunkedBody = chunks
        .map((chunk) => {
            const size = responseEncoder.encode(chunk).length.toString(16);
            return `${size}\r\n${chunk}\r\n`;
        })
        .join("");

    return {
        path,
        body,
        length,
        responses: [
            `HTTP/1.1 200 OK\r\nContent-Length: ${length}\r\n\r\n${body}`,
            `HTTP/1.1 200 OK\r\nTransfer-Encoding: chunked\r\n\r\n${chunkedBody}0\r\n\r\n`,
        ],
    };
});
export function KeepAlive() {
    const [selected, select] = useState(0);
    const [framing, setFraming] = useState(0);
    const { body, length } = messages[selected];

    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>Одне TCP-з’єднання</strong>
                <span>
                    {framing === 0
                        ? "Content-Length → межа тіла в байтах"
                        : "Розміри чанків у hex · 0 і порожній рядок завершують відповідь"}
                </span>
            </div>
            <Choices
                name="Формат тіла відповіді"
                labels={["Keep-Alive + Content-Length", "Keep-Alive + Chunked"]}
                selected={framing}
                onSelect={setFraming}
            />
            <div className="raw-pair">
                <div>
                    <h2>Запити клієнта</h2>
                    {messages.map(({ path }, i) => (
                        <button
                            key={path}
                            className="raw-request"
                            aria-pressed={selected === i}
                            onClick={() => select(i)}
                        >
                            <code>
                                GET {path} HTTP/1.1
                                <br />
                                Host: shop.test
                                <br />
                                <br />
                            </code>
                            <b aria-hidden="true">→</b>
                        </button>
                    ))}
                </div>
                <div>
                    <h2>Суцільний потік відповідей</h2>
                    <pre className="raw-responses">
                        {messages.map(({ path, responses }, i) => (
                            <span
                                key={path}
                                className={selected === i ? "marked" : ""}
                            >
                                {responses[framing]}
                            </span>
                        ))}
                    </pre>
                </div>
            </div>
            <p className="protocol-caption">
                Запит {selected + 1} → відповідь {selected + 1} · тіло «{body}»
                · {length} Б
            </p>
        </div>
    );
}

export function HttpQueue() {
    const [slow, setSlow] = useState(1);
    const [tick, setTick] = useState(2);
    const ready = [slow ? 4 : 1, 1, 2];
    const delivered = ready.map((_, i) => Math.max(...ready.slice(0, i + 1)));
    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>HTTP/1.1 · pipelining</strong>
                <span>Три GET поспіль · одне з’єднання</span>
            </div>
            <Choices
                name="Час першої відповіді"
                labels={["Швидка перша відповідь", "Повільна перша відповідь"]}
                selected={slow}
                onSelect={setSlow}
            />
            <div className="queue-grid">
                <b>Запит</b>
                <b>На сервері</b>
                <b>У браузері · t = {tick}</b>
                {["/report", "/logo", "/menu"].map((path, i) => (
                    <div className="queue-row" key={path}>
                        <code>GET {path}</code>
                        <span>
                            {tick < ready[i]
                                ? "Обчислення…"
                                : "Готово · t = " + ready[i]}
                        </span>
                        <strong
                            data-state={
                                tick >= delivered[i]
                                    ? "done"
                                    : tick >= ready[i]
                                      ? "wait"
                                      : ""
                            }
                        >
                            {tick >= delivered[i]
                                ? "✓ Доставлено " + (i + 1)
                                : tick >= ready[i]
                                  ? "За першою відповіддю"
                                  : "Очікування"}
                        </strong>
                    </div>
                ))}
            </div>
            <Steps step={tick} last={4} onStep={setTick} next="Такт" />
        </div>
    );
}
