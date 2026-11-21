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
                  "Що саме змінив ECH",
                  "ECH шифрує внутрішній ClientHello відкритим ключем сервера, який браузер отримав заздалегідь, зазвичай із DNS. Сервер може розшифрувати ім’я, спостерігач — ні. HTTP-запит шифрується і з ECH, і без нього.",
              ]
            : [
                  "HTTPS без ECH: сайт видно, сторінку — ні",
                  "Провайдер читає example.com у полі SNI першого повідомлення TLS. Шлях /orders і HTTP-заголовок Host уже зашифровані. Домен видає саме SNI, а не Host.",
              ];
    }

    return mode === "http"
        ? [
              "HTTP без шифрування",
              "У мережі видно і Host: example.com, і GET /orders. Додавання HTTPS приховає цей запит, але без ECH ім’я сайту все ще читатиметься у привітанні TLS.",
          ]
        : [
              "Host усередині зашифрованого HTTP",
              "Host і SNI передають те саме ім’я на різних етапах. Host потрібен HTTP-серверу після встановлення TLS; SNI потрібен раніше, під час налаштування TLS. Без ECH саме ця рання копія імені лишається відкритою.",
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
            <h2>Де саме видно домен у HTTPS?</h2>
            <p className="host-intro">
                Те саме ім’я сайту є у <strong>двох різних місцях</strong>: SNI
                у привітанні TLS та Host у HTTP-запиті. HTTPS без ECH шифрує
                лише другу з цих двох копій.
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
                <strong>
                    ECH = Encrypted Client Hello — «зашифроване привітання
                    клієнта».
                </strong>{" "}
                Розширення TLS 1.3: шифрує внутрішнє початкове повідомлення
                браузера разом зі справжнім ім’ям сайту. Перемкни режим і
                порівняй перший крок.
            </p>
            <div>
                {tls && (
                    <section className="host-phase">
                        <h3>1. Спочатку — «до якого сайту підключаємося?»</h3>
                        <p>
                            <strong>ClientHello</strong> — перше повідомлення
                            браузера в TLS. <strong>SNI</strong> (Server Name
                            Indication) — поле з іменем сайту, щоб сервер міг
                            вибрати потрібний сертифікат.
                        </p>
                        <Flow
                            browser={
                                <Endpoint
                                    title="Браузер готує привітання"
                                    code={"ClientHello\nSNI: example.com"}
                                    note={
                                        ech
                                            ? "ECH шифрує внутрішнє привітання до відправлення."
                                            : "Без ECH це привітання йде відкрито."
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
                                                Умовне спільне ім’я провайдера,
                                                а не сайту.
                                            </small>
                                        </>
                                    ) : (
                                        <>
                                            <small>
                                                ClientHello → поле SNI
                                            </small>
                                            <code>example.com</code>
                                            <b>Ось тут домен видно відкрито.</b>
                                            <small>
                                                Це можна прочитати без ключа
                                                шифрування.
                                            </small>
                                        </>
                                    )}
                                </Observer>
                            }
                            server={
                                <Endpoint
                                    title="Сервер отримує ім’я"
                                    code="example.com"
                                    note={
                                        ech
                                            ? "Розшифровує внутрішній ClientHello ключем ECH."
                                            : "Читає SNI та вибирає сертифікат сайту."
                                    }
                                />
                            }
                        />
                    </section>
                )}
                <section className="host-phase">
                    <h3>
                        {tls
                            ? "2. Потім — «дай сторінку /orders»"
                            : "HTTP · запит одразу йде відкрито"}
                    </h3>
                    <p>
                        {tls ? (
                            <>
                                TLS уже узгодив ключі сеансу.{" "}
                                <strong>
                                    Тепер увесь HTTP-запит шифрується
                                </strong>
                                , включно із заголовком Host.
                            </>
                        ) : (
                            "Без TLS немає ні захищеного привітання, ні шифрування HTTP-запиту."
                        )}
                    </p>
                    <Flow
                        browser={
                            <Endpoint
                                title="Браузер готує HTTP-запит"
                                code={request}
                                note={
                                    tls
                                        ? "Шифрує запит ключами сеансу TLS."
                                        : "Передає текст у TCP без шифрування."
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
                                        <b>
                                            Ані /orders, ані Host не читаються.
                                        </b>
                                        <small>
                                            У мережі — шифротекст. Зміст
                                            доступний лише кінцевим сторонам із
                                            ключами.
                                        </small>
                                    </>
                                ) : (
                                    <>
                                        <code>Host: example.com</code>
                                        <b>Домен і шлях /orders відкриті.</b>
                                        <small>
                                            Увесь HTTP-запит можна прочитати без
                                            ключа.
                                        </small>
                                    </>
                                )}
                            </Observer>
                        }
                        server={
                            <Endpoint
                                title="HTTP-сервер читає запит"
                                code={request}
                                note={
                                    tls
                                        ? "Після розшифрування TLS."
                                        : "Той самий відкритий текст."
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
                <strong>Коли з’явився захист імені?</strong>{" "}
                <a
                    href="https://blog.cloudflare.com/encrypted-sni/"
                    target="_blank"
                    rel="noopener"
                >
                    2018 — експериментальний ESNI (Encrypted SNI)
                </a>
                ;{" "}
                <a
                    href="https://blog.cloudflare.com/announcing-encrypted-client-hello/"
                    target="_blank"
                    rel="noopener"
                >
                    2023 — впровадження ECH у Cloudflare
                </a>
                ;{" "}
                <a
                    href="https://www.rfc-editor.org/rfc/rfc9849.html"
                    target="_blank"
                    rel="noopener"
                >
                    березень 2026 — стандарт ECH
                </a>
                . ECH має підтримуватися клієнтом і сервером та успішно
                узгодитися.
            </p>
            <p className="traffic-boundaries">
                Схема показує вміст повідомлень, а не межі пакетів. Навіть з ECH
                видно IP-адреси, розміри й час передавання; незашифрований DNS
                також може видати домен. Host з’явився у HTTP/1.1 у 1997-му;{" "}
                <a
                    href="https://www.rfc-editor.org/rfc/rfc3546.html#section-3.1"
                    target="_blank"
                    rel="noopener"
                >
                    SNI стандартизували у 2003-му
                </a>
                . Тут порівнюємо їхню подальшу роботу в HTTPS.
            </p>
        </div>
    );
}
