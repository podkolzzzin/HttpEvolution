import { useId, useState } from "react";
import "../styles/tls-negotiation.css";

const handshakeChanges = [
    [
        {
            title: "Повний обмін ключами займав 2 RTT",
            detail: "Клієнт спершу чекав на параметри сервера, потім надсилав дані для обміну ключами й чекав на підтвердження.",
        },
        {
            title: "1 RTT, якщо сервер прийняв ключ клієнта",
            detail: "Клієнт надсилає відкритий ключ уже в першому повідомленні. Після відповіді сервера він може надіслати HTTP-запит.",
        },
    ],
    [
        {
            title: "Витік ключа RSA ставив під загрозу записані сеанси",
            detail: "Клієнт шифрував секрет відкритим ключем сервера. Парний закритий ключ міг розкрити цей секрет навіть через роки.",
        },
        {
            title: "Тимчасові ключі захищають старі сеанси",
            detail: "Клієнт і сервер створюють нові ключі ECDHE для кожного сеансу. Ключ сертифіката лише засвідчує обмін.",
        },
    ],
    [
        {
            title: "CBC вимагає окремої перевірки",
            detail: "CBC приховує вміст запису. Щоб виявити підміну, отримувач окремо перевіряє код автентичності (MAC).",
        },
        {
            title: "AEAD шифрує та перевіряє разом",
            detail: "Під час шифрування AEAD створює тег. Якщо тег не збігається під час розшифрування, отримувач відкидає запис.",
        },
    ],
];

export function TlsChanges() {
    return (
        <div className="tls-changes" aria-label="Зміни в TLS 1.3">
            <div className="tls-change-head">
                <span>Було</span>
                <span>Стало в TLS 1.3</span>
            </div>
            {handshakeChanges.map((pair) => (
                <div className="tls-change-row" key={pair[0].title}>
                    {pair.map(({ title, detail }, index) => (
                        <div key={title}>
                            <span className={index === 0 ? "tls-minus" : "tls-plus"} aria-hidden="true">
                                {index === 0 ? "−" : "+"}
                            </span>
                            <strong>{title}</strong>
                            <p>{detail}</p>
                        </div>
                    ))}
                </div>
            ))}
            <p className="tls-footnote">
                TLS 1.2 також підтримував обмін тимчасовими ключами та
                шифрування з перевіркою цілісності.
            </p>
        </div>
    );
}

export function TlsExchange() {
    return (
        <div className="tls-exchange">
            <section className="tls-exchange-case tls-rsa">
                <header>
                    <strong>Передавання секрету RSA</strong>
                    <span>старий підхід</span>
                </header>
                <div className="tls-flow-figure">
                    <div className="tls-endpoints">
                        <b>Клієнт</b>
                        <b>Сервер</b>
                    </div>
                    <div className="tls-wire-row">
                        <span>Зашифрований секрет</span>
                        <i aria-label="від клієнта до сервера">→</i>
                    </div>
                    <div className="tls-local-row">
                        <span>Клієнт створює секрет</span>
                        <span>Сервер розшифровує його закритим ключем</span>
                    </div>
                </div>
                <p>
                    Якщо згодом викрадуть приватний ключ сервера, записаний
                    handshake може розкрити старі сеанси.
                </p>
            </section>
            <section className="tls-exchange-case tls-ecdhe">
                <header>
                    <strong>Тимчасові ключі ECDHE</strong>
                    <span>TLS 1.3</span>
                </header>
                <div className="tls-flow-figure">
                    <div className="tls-endpoints">
                        <b>Клієнт</b>
                        <b>Сервер</b>
                    </div>
                    <div className="tls-wire-row">
                        <span>Відкритий ключ клієнта A</span>
                        <i aria-label="від клієнта до сервера">→</i>
                    </div>
                    <div className="tls-wire-row reverse">
                        <span>Відкритий ключ сервера B + сертифікат</span>
                        <i aria-label="від сервера до клієнта">←</i>
                    </div>
                    <div className="tls-local-row">
                        <span>
                            Локально: закритий ключ клієнта + B<br />
                            <strong>→ спільний секрет</strong>
                        </span>
                        <span>
                            Локально: закритий ключ сервера + A<br />
                            <strong>→ той самий секрет</strong>
                        </span>
                    </div>
                </div>
                <p>
                    Ефемерні секрети видаляються після сеансу. Майбутній витік
                    довгострокового ключа сертифіката не розкриває записаний
                    трафік.
                </p>
            </section>
            <p className="tls-exchange-note">
                В ECDHE обидві сторони обчислюють секрет локально. Сертифікат і
                підпис прив’язують обмін до сервера.
            </p>
        </div>
    );
}

export function TlsFinished() {
    const [match, setMatch] = useState(true);
    return (
        <div className="tls-finished">
            <ol className="finished-timeline" aria-label="Від рукостискання до HTTP-запиту">
                <li className="finished-context">
                    <span className="finished-phase">До Finished</span>
                    <strong>Обмін ключами</strong>
                    <p>Браузер і сервер уже обчислили спільний секрет.</p>
                </li>
                <li className="finished-check">
                    <span className="finished-direction">Сервер → браузер</span>
                    <strong className="finished-message">Finished</strong>
                    <p>Сервер надсилає код, обчислений із ключа та історії рукостискання. Браузер звіряє його зі своїм розрахунком.</p>
                    <div className="finished-switch" role="group" aria-label="Історія рукостискання">
                        <button type="button" aria-pressed={match} onClick={() => setMatch(true)}>
                            Однакова історія
                        </button>
                        <button type="button" aria-pressed={!match} onClick={() => setMatch(false)}>
                            Змінити 1 байт
                        </button>
                    </div>
                    <div className="finished-comparison" role="status">
                        <code>7A…31 {match ? "= 7A…31" : "≠ C4…90"}</code>
                        <span>{match ? "Коди збіглися. Браузер надсилає свій Finished." : "Коди різні. Браузер перериває рукостискання."}</span>
                    </div>
                </li>
                <li className="finished-context">
                    <span className="finished-phase">{match ? "Після Finished" : "Помилка перевірки"}</span>
                    <strong>{match ? "Перший HTTP-запит" : "З’єднання перервано"}</strong>
                    {match ? (
                        <p>Браузер надсилає <code>GET /</code> через те саме захищене з’єднання.</p>
                    ) : (
                        <p>Браузер не надсилає HTTP-запит.</p>
                    )}
                </li>
            </ol>
        </div>
    );
}

export function AlpnChoice() {
    const [h2, setH2] = useState(true);
    const resultId = useId();
    return (
        <div className="tls-alpn">
            <div className="tls-alpn-flow">
                <div className="tls-alpn-peer">
                    <small>Браузер пропонує в TLS</small>
                    <code>h2, http/1.1</code>
                </div>
                <span className="tls-alpn-arrow" aria-hidden="true">
                    →
                </span>
                <div className="tls-alpn-peer">
                    <small>Сервер підтримує</small>
                    <div
                        className="tls-finished-controls"
                        role="group"
                        aria-label="Можливості сервера"
                    >
                        <button
                            type="button"
                            aria-pressed={h2}
                            onClick={() => setH2(true)}
                        >
                            h2 + HTTP/1.1
                        </button>
                        <button
                            type="button"
                            aria-pressed={!h2}
                            onClick={() => setH2(false)}
                        >
                            лише HTTP/1.1
                        </button>
                    </div>
                </div>
                <span className="tls-alpn-arrow" aria-hidden="true">
                    →
                </span>
                <div className="tls-alpn-selected">
                    <small>Сервер вибирає спільний</small>
                    <strong>{h2 ? "h2" : "http/1.1"}</strong>
                </div>
            </div>
            <div className="tls-alpn-output" id={resultId}>
                <small>
                    Дані всередині захищеного каналу · формат{" "}
                    {h2 ? "HTTP/2" : "HTTP/1.1"}
                </small>
                {h2 ? (
                    <>
                        <code className="tls-preface">
                            PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n
                        </code>
                        <span>далі — двійкові кадри HTTP/2</span>
                    </>
                ) : (
                    <code>GET / HTTP/1.1\r\nHost: example.com\r\n…</code>
                )}
            </div>
            <p className="tls-footnote">
                Так працює ALPN. Сервер повертає свій вибір під час
                TLS-рукостискання, ще до першого HTTP-запиту.
            </p>
        </div>
    );
}
