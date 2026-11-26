import { useState } from "react";
import "./QuicWorkbench.css";
import { HighlightedCode } from "../slides/HighlightedCode";

const cx = (...parts: Array<string | false>) => parts.filter(Boolean).join(" ");

export function QuicProtection() {
    const [tampered, setTampered] = useState(false);
    const [step, setStep] = useState<"route" | "message">("route");

    function reset() {
        setTampered(false);
        setStep("route");
    }

    return (
        <section className="qw qw-protection" aria-label="Захист QUIC">
            <div className="qw-heading">
                <p>
                    1-RTT пакет: IP маршрутизує за адресами; endpoint або
                    балансувальник використовує DCID, щоб знайти з’єднання.
                </p>
                <button className="qw-reset" type="button" onClick={reset}>
                    Скинути
                </button>
            </div>
            <div
                className="qw-controls"
                role="group"
                aria-label="Виберіть частину пакета"
            >
                <button
                    type="button"
                    aria-pressed={step === "route"}
                    onClick={() => setStep("route")}
                >
                    Заголовок для маршруту
                </button>
                <button
                    type="button"
                    aria-pressed={step === "message"}
                    onClick={() => setStep("message")}
                >
                    Повідомлення
                </button>
                <button
                    type="button"
                    aria-pressed={tampered}
                    onClick={() => setTampered(!tampered)}
                >
                    {tampered ? "Повернути байти" : "Змінити шифротекст"}
                </button>
            </div>
            <div className="qw-protect-grid">
                <div className="qw-endpoint">
                    <span className="qw-node">A</span>
                    <strong>Відправник</strong>
                    <small>шифрує й автентифікує</small>
                </div>
                <div className="qw-wire">
                    <div className="qw-packet">
                        <button
                            type="button"
                            className={cx(
                                "qw-field",
                                step === "route" && "is-active",
                            )}
                            onClick={() => setStep("route")}
                        >
                            <small>Відкритий short header</small>
                            <code>DCID · 8f2a…</code>
                            <span>пошук з’єднання на endpoint</span>
                        </button>
                        <button
                            type="button"
                            className={cx(
                                "qw-field qw-sealed",
                                step === "message" && "is-active",
                                tampered && "is-tampered",
                            )}
                            onClick={() => setStep("message")}
                        >
                            <small>Захищена частина</small>
                            <code>
                                {tampered ? "c4 91 7e … !" : "c4 91 7e …"}
                            </code>
                            <span>
                                {tampered
                                    ? "байти змінено"
                                    : "HTTP/3 · STREAM · шифротекст"}
                            </span>
                        </button>
                    </div>
                    <div className="qw-observer">
                        <span aria-hidden="true">◉</span>
                        <strong>Спостерігач</strong>
                        <small>
                            {step === "route"
                                ? "бачить DCID; IP-маршрут визначають адреси"
                                : tampered
                                  ? "бачить зміну, але не читає вміст"
                                  : "бачить лише зашифровані байти"}
                        </small>
                    </div>
                    <div className="qw-wireline" aria-hidden="true" />
                </div>
                <div className="qw-endpoint">
                    <span className="qw-node qw-node-recipient">B</span>
                    <strong>Одержувач</strong>
                    <small>
                        {tampered
                            ? "перевірка не пройшла · відкинуто"
                            : "перевіряє й розшифровує"}
                    </small>
                </div>
            </div>
            <p
                className={cx("qw-status", tampered && "qw-status-warn")}
                role="status"
            >
                {tampered
                    ? "Автентифікація не збігається. Одержувач відкидає пакет."
                    : step === "route"
                      ? "DCID і IP-адреси відкриті; HTTP/3-вміст зашифрований."
                      : "Пакет автентичний; одержувач може розшифрувати вміст."}
            </p>
            <div className="qw-benefits">
                <span>
                    <b>Приватність</b> · вміст прихований
                </span>
                <span>
                    <b>Цілісність</b> · підміну виявлено
                </span>
            </div>
        </section>
    );
}

const offsetPackets = [
    { label: "offset 0 · ABCD", range: [0, 4] as const, fin: false },
    { label: "offset 4 · EFGH + FIN", range: [4, 8] as const, fin: true },
    { label: "offset 2 · CDEF", range: [2, 6] as const, fin: false },
    { label: "повтор · ABCD", range: [0, 4] as const, fin: false },
];

const offsetPseudocode = `var s = streams.GetOrCreate(f.StreamId);
for (int i = 0; i < f.Data.Length; i++)
    s.Bytes.TryAdd(f.Offset + i, f.Data[i]);

while (s.Bytes.TryGetValue(s.Next, out byte b))
{
    s.Deliver(b);
    s.Next++;
}

if (f.Fin) s.FinalSize = f.Offset + f.Data.Length;
if (s.Next == s.FinalSize) s.FinishOnce();`;

type ByteRange = readonly [number, number];

function mergeRanges(ranges: ByteRange[]) {
    const merged: [number, number][] = [];
    for (const [start, end] of [...ranges].sort((a, b) => a[0] - b[0])) {
        const last = merged.at(-1);
        if (last && start <= last[1]) last[1] = Math.max(end, last[1]);
        else merged.push([start, end]);
    }
    return merged;
}

const streamBytes = Array.from("ABCDEFGH");

function describeArrival([start, end]: ByteRange, counts: number[]) {
    const bytes = streamBytes.slice(start, end);
    const repeated = bytes.filter((_, i) => counts[start + i] > 1).join("");
    const added = bytes.filter((_, i) => counts[start + i] === 1).join("");
    if (!added) return `Усі байти ${repeated} вже є на цих позиціях. Це повний повтор.`;
    if (!repeated) return `Усі байти ${added} надійшли вперше.`;
    return `Байти ${repeated} перекривають уже отримані позиції. Кадр додає ${added}.`;
}

export function QuicOffsets() {
    const [received, setReceived] = useState<number[]>([]);
    const frames = received.map((i) => offsetPackets[i]);
    const ranges = frames.map((f) => f.range);
    const merged = mergeRanges(ranges);
    const counts = streamBytes.map((_, i) =>
        ranges.filter(([start, end]) => start <= i && i < end).length,
    );
    const lastRange = ranges.at(-1);
    const finalSize = frames.find((f) => f.fin)?.range[1];
    const prefixEnd = merged[0]?.[0] === 0 ? merged[0][1] : 0;
    const complete = prefixEnd === finalSize;
    const hasGap = merged.some(([start]) => start > prefixEnd);

    function receive(index: number) {
        setReceived((current) =>
            current.includes(index) ? current : [...current, index],
        );
    }
    function reset() {
        setReceived([]);
    }

    return (
        <section
            className="qw qw-offsets"
            aria-label="Збирання даних STREAM за зсувами"
        >
            <div className="qw-heading">
                <p>
                    Додайте кадри STREAM у будь-якому порядку. Offset вказує
                    позицію першого байта кадру в потоці 0.
                </p>
                <button className="qw-reset" type="button" onClick={reset}>
                    Скинути
                </button>
            </div>
            <div
                className="qw-controls qw-packet-controls"
                role="group"
                aria-label="Додати кадр STREAM"
            >
                {offsetPackets.map((packet, index) => (
                    <button
                        key={index}
                        type="button"
                        aria-pressed={received.includes(index)}
                        onClick={() => receive(index)}
                        disabled={received.includes(index)}
                    >
                        {packet.label}
                    </button>
                ))}
            </div>
            <div className="qw-offset-layout">
                <div className="qw-buffer">
                    <div className="qw-buffer-top">
                        <strong>Буфер · потік 0</strong>
                        <span>
                            {complete
                                ? "потік завершений"
                                : hasGap
                                  ? "є пропуск у байтах"
                                  : prefixEnd > 0
                                    ? "префікс доступний застосунку"
                                    : "очікує дані"}
                        </span>
                    </div>
                    <div className="qw-byte-ruler" aria-hidden="true">
                        {Array.from({ length: 9 }, (_, i) => (
                            <span key={i}>{i}</span>
                        ))}
                    </div>
                    <div
                        className="qw-range-track"
                        role="list"
                        aria-label="Байти потоку 0"
                    >
                        {streamBytes.map((byte, i) => {
                            const count = counts[i];
                            const status = count > 1
                                ? "отримано повторно"
                                : count === 1 ? "отримано" : "ще не отримано";
                            return (
                                <span
                                    key={i}
                                    role="listitem"
                                    className={cx(
                                        "qw-byte",
                                        count > 0 && "is-filled",
                                        count > 1 && "is-overlap",
                                    )}
                                    aria-label={`Позиція ${i}. Байт ${byte}, ${status}.`}
                                >
                                    {count ? byte : "·"}
                                </span>
                            );
                        })}
                    </div>
                    <div className="qw-ranges">
                        {merged.length ? (
                            merged.map(([a, b]) => (
                                <span key={a}>
                                    [{a}, {b})
                                </span>
                            ))
                        ) : (
                            <span className="qw-empty">
                                діапазонів поки немає
                            </span>
                        )}
                    </div>
                    <div
                        className={cx("qw-fin", finalSize !== undefined && "is-set")}
                    >
                        FIN{" "}
                        {finalSize === undefined
                            ? "не отримано · кінцевий розмір невідомий"
                            : `· final size = ${finalSize}`}
                    </div>
                    <div
                        className={cx(
                            "qw-app-read",
                            prefixEnd > 0 && "is-ready",
                        )}
                    >
                        {complete
                            ? "Застосунок отримав ABCDEFGH. Потік завершено."
                            : prefixEnd > 0
                              ? `Застосунок отримав ${streamBytes.slice(0, prefixEnd).join("")} і чекає на наступні байти.`
                              : "Застосунок чекає на початок потоку."}
                    </div>
                    <p className="qw-packet-note" role="status">
                        {lastRange && describeArrival(lastRange, counts)}
                    </p>
                </div>
                <div className="qw-code-panel">
                    <div className="qw-code-title">
                        C# · псевдокод обробки одного кадру STREAM
                    </div>
                    <pre>
                        <HighlightedCode code={offsetPseudocode} />
                    </pre>
                    <p>
                        TryAdd зберігає байт лише при першій появі його зсуву.
                        Next починається з нуля і позначає наступний байт для застосунку.
                        До FIN значення FinalSize дорівнює null.
                    </p>
                </div>
            </div>
        </section>
    );
}

type LimitKey = "connection" | "stream";
const initialLimits = { connection: 6, stream: 4 };

export function QuicFlow() {
    const [limits, setLimits] = useState(initialLimits);
    const [read, setRead] = useState(false);
    const target = 8;
    const canSend = Math.min(limits.connection, limits.stream) >= target;
    function grant(key: LimitKey) {
        setLimits((current) => ({ ...current, [key]: 8 }));
    }
    function receiverRead() {
        setRead(true);
        setLimits({ connection: 12, stream: 12 });
    }
    function reset() {
        setLimits(initialLimits);
        setRead(false);
    }
    return (
        <section className="qw qw-flow" aria-label="Керування потоком QUIC">
            <div className="qw-heading">
                <p>
                    Відправник має дотриматися обох абсолютних меж у кожному
                    напрямку.
                </p>
                <button className="qw-reset" type="button" onClick={reset}>
                    Скинути
                </button>
            </div>
            <div className="qw-flow-stage">
                <div className="qw-flow-end">
                    <span className="qw-node">A</span>
                    <strong>Відправник</strong>
                    <small>потік 0 · дані до 8</small>
                </div>
                <div className="qw-flow-middle">
                    <div className="qw-tp">
                        <strong>TLS handshake · транспортні параметри</strong>
                        <span>
                            Сервер оголошує клієнту, скільки даних готовий
                            прийняти. Потік 0 відкрив клієнт.
                        </span>
                        <code>initial_max_data = 6</code>
                        <code>initial_max_stream_data_bidi_remote = 4</code>
                    </div>
                    <div className="qw-flow-arrows">
                        <span>STREAM · відправлення даних ↓</span>
                        <span>MAX_* · нові межі ↑</span>
                    </div>
                    <div className="qw-limit-controls">
                        <div>
                            <label>MAX_DATA · з’єднання</label>
                            <strong>{limits.connection} байтів</strong>
                            <button
                                type="button"
                                onClick={() => grant("connection")}
                                disabled={limits.connection >= 8}
                            >
                                {limits.connection >= 12
                                    ? "MAX_DATA 12 надіслано"
                                    : limits.connection >= 8
                                      ? "MAX_DATA 8 надіслано"
                                      : "Надіслати MAX_DATA 8"}
                            </button>
                        </div>
                        <div>
                            <label>MAX_STREAM_DATA · потік 0</label>
                            <strong>{limits.stream} байтів</strong>
                            <button
                                type="button"
                                onClick={() => grant("stream")}
                                disabled={limits.stream >= 8}
                            >
                                {limits.stream >= 12
                                    ? "MAX_STREAM_DATA 12 надіслано"
                                    : limits.stream >= 8
                                      ? "MAX_STREAM_DATA 8 надіслано"
                                      : "Надіслати MAX_STREAM_DATA 8"}
                            </button>
                        </div>
                    </div>
                </div>
                <div className="qw-flow-end">
                    <span className="qw-node qw-node-recipient">B</span>
                    <strong>Одержувач</strong>
                    <small>
                        {read
                            ? "застосунок прочитав дані"
                            : "буфер приймає дані"}
                    </small>
                </div>
            </div>
            <div className="qw-send-state">
                <div className="qw-send-bar">
                    <span
                        style={{
                            width: `${Math.min(100, (Math.min(limits.connection, limits.stream) / target) * 100)}%`,
                        }}
                    />
                </div>
                <p role="status">
                    {read
                        ? "Застосунок прочитав 4 байти; MAX_DATA і MAX_STREAM_DATA підняли межу для відправника до 12."
                        : canSend
                          ? "Обидві межі — 8: відправник може продовжити до цього ліміту."
                          : `Надіслано до ${Math.min(limits.connection, limits.stream)} з 8 байтів; стримує ${limits.connection <= limits.stream ? "межа з’єднання MAX_DATA" : "межа потоку MAX_STREAM_DATA"}.`}
                </p>
                <button
                    type="button"
                    onClick={receiverRead}
                    disabled={!canSend || read}
                >
                    {read
                        ? "MAX_DATA / MAX_STREAM_DATA = 12"
                        : "Прочитати 4 байти й відкрити вікно"}
                </button>
            </div>
            <p className="qw-note">
                Для зворотного напрямку одержувач надсилає власні межі; кожен
                напрямок має окремі транспортні параметри й ліміти.
            </p>
        </section>
    );
}
