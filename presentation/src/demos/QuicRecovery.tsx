import { useState } from "react";
import "./QuicRecovery.css";

const stages = [
    {
        title: "Пропуск",
        detail: "Пакет 11 загубився. Для відправника це поки виглядає так само, як звичайна затримка.",
    },
    {
        title: "Поріг втрати",
        detail: "Найбільший підтверджений номер — 14. Різниця 14 − 11 = 3 досягає початкового порога. Відправник вважає пакет 11 втраченим.",
    },
    {
        title: "Повтор байтів",
        detail: "Відправник бере новий номер 15. Байти CD зберігають зсув 2, тож одержувач заповнює прогалину в потоці.",
    },
];

type FlightKind = "lost" | "later" | "ack" | "retry";

function Flight({ kind, label }: { kind: FlightKind; label: string }) {
    // The diagram's horizontal scale stretches to the distance between peers.
    const reverse = kind === "ack";
    const end = kind === "lost" ? 44 : reverse ? 0 : 100;
    const start = reverse ? 100 : 0;
    const rows = kind === "later" ? [0, 6, 12] : [0];
    return (
        <div className={`qr-flight qr-${kind}`}>
            <span className="qr-flight-label">{label}</span>
            <svg viewBox="0 0 100 44" preserveAspectRatio="none" aria-hidden="true">
                {rows.map((offset) => (
                    <line key={offset} x1={start} y1={7 + offset} x2={end} y2={30 + offset} />
                ))}
            </svg>
            {kind === "lost" ? <b className="qr-cross" aria-hidden="true">×</b> :
                rows.map((offset) => (
                    <i key={offset} className="qr-arrival" aria-hidden="true" style={{ top: `${(30 + offset) / 44 * 100}%` }} />
                ))}
        </div>
    );
}

export function QuicRecovery() {
    const [focus, setFocus] = useState<number | null>(null);

    return (
        <section className="qr-panel" aria-label="Виявлення втрати й повторна передача даних">
            <figure className="qr-diagram" aria-label="Рух пакетів у часі. Обмін показано згори вниз.">
                <figcaption className="qr-numbering">
                    <strong>PN (Packet Number) — номер пакета</strong>
                    <p>Кожна сторона нумерує свої пакети від 0 окремо в кожному просторі.
                        Номери строго зростають, пропуски дозволені. Після 5 можна взяти 7, далі — лише 8 або більше.</p>
                </figcaption>
                <div className="qr-peers">
                    <strong>Відправник</strong><span>Час ↓</span><strong>Одержувач</strong>
                </div>
                <div className="qr-trace">
                    <div data-muted={focus !== null && focus !== 0}>
                        <Flight kind="lost" label="PN 11 · CD · зсув 2" />
                    </div>
                    <div data-muted={focus !== null && focus !== 1}>
                        <Flight kind="later" label="PN 12, 13, 14" />
                        <Flight kind="ack" label="PN 4 · ACK 12–14" />
                    </div>
                    <div data-muted={focus !== null && focus !== 2}>
                        <Flight kind="retry" label="PN 15 · CD · зсув 2" />
                    </div>
                </div>
            </figure>
            <div className="qr-stages" role="group" aria-label="Виділити подію на схемі">
                {stages.map((stage, index) => (
                    <button
                        key={stage.title}
                        type="button"
                        className={`qr-stage qr-stage-${index}`}
                        aria-pressed={focus === index}
                        onClick={() => setFocus(focus === index ? null : index)}
                    >
                        <strong>{stage.title}</strong>
                        <span>{stage.detail}</span>
                    </button>
                ))}
            </div>
            <p className="qr-spaces">
                <strong>Три простори номерів</strong> — Initial, Handshake і Application Data (спільний для 0-RTT та 1-RTT).
                На схемі пакет одержувача з PN 4 підтверджує пакети 12–14 відправника в просторі Application Data.{" "}
                <a href="https://www.rfc-editor.org/rfc/rfc9000.html#section-12.3" target="_blank" rel="noopener">RFC 9000 §12.3</a>
            </p>
        </section>
    );
}
