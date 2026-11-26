import { useLayoutEffect, useRef, useState } from "react";
import "./Continuation.css";
import { HighlightedCode } from "../slides/HighlightedCode";
import { Choices, Steps } from "./ProtocolControls";

const frames = [
    { id: 1, data: "<h1>", end: false },
    { id: 3, data: "bo", end: false },
    { id: 3, data: "dy", end: true },
    { id: 1, data: "Hi</h1>", end: true },
];

const assemblyCode = `var body = bodies[frame.Id];
body.Write(frame.Data);
// Чи встановлено END_STREAM (біт 0)?
if ((frame.Flags & 1) != 0)
    Deliver(frame.Id, body.ToArray());`;

export function StreamAssembly() {
    const [step, setStep] = useState(0);
    const consumed = frames.slice(0, step);
    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>Один TCP · чотири DATA-кадри</strong>
                <span>Після HEADERS для потоків 1 і 3</span>
            </div>
            <div className="frame-tape">
                {frames.map((f, i) => (
                    <button
                        key={i}
                        data-lane={f.id}
                        aria-pressed={step === i + 1}
                        onClick={() => setStep(i + 1)}
                    >
                        <small>DATA · ID {f.id}</small>
                        <code>{f.data}</code>
                        <small>{f.end ? "END_STREAM" : "ще дані"}</small>
                    </button>
                ))}
            </div>
            <div className="assembly-grid">
                <div>
                    {[1, 3].map((id) => (
                        <div className="stream-output" key={id} data-lane={id}>
                            <b>Буфер #{id}</b>
                            <code>
                                {consumed
                                    .filter((x) => x.id === id)
                                    .map((x) => x.data)
                                    .join("") || "∅"}
                            </code>
                            <small>
                                {consumed.some((x) => x.id === id && x.end)
                                    ? "✓ Тіло зібрано"
                                    : "Відкритий потік"}
                            </small>
                        </div>
                    ))}
                </div>
                <div className="assembly-code">
                    <b>Приймач · C# · для DATA</b>
                    <pre>
                        <HighlightedCode code={assemblyCode} />
                    </pre>
                </div>
            </div>
            <Steps
                step={step}
                last={4}
                onStep={setStep}
                next="Прочитати кадр"
            />
        </div>
    );
}

const headerStart = { stream: 1, kind: "HEADERS", bytes: "83 86", note: "Фрагмент 1/3" };
const headerMiddle = { stream: 1, kind: "CONTINUATION", bytes: "84", note: "Фрагмент 2/3" };
const headerEnd = { stream: 1, kind: "CONTINUATION", bytes: "01 01 61", note: "END_HEADERS" };
const nextRequest = { stream: 3, kind: "HEADERS", bytes: "83 86 84 01 01 62", note: "END_HEADERS" };
const firstBody = { stream: 1, kind: "DATA", bytes: "41", note: "END_STREAM" };
const nextBody = { stream: 3, kind: "DATA", bytes: "42", note: "END_STREAM" };
const validOrder = [headerStart, headerMiddle, headerEnd, nextRequest, firstBody, nextBody];
const interruptedOrder = [headerStart, headerMiddle, nextRequest, headerEnd, firstBody, nextBody];
const receiverNotes = [
    "Приймач чекає на перший кадр.",
    "HEADERS відкриває потік 1. Приймач чекає на CONTINUATION потоку 1.",
    "Блок ще відкритий. Наступним має бути CONTINUATION потоку 1.",
    "END_HEADERS завершує блок потоку 1. Декодовано запит POST / до хоста a.",
    "HEADERS відкриває потік 3. Тепер обидва потоки відкриті.",
    "DATA завершує тіло запиту в потоці 1. Потік 3 ще відкритий.",
    "Після END_HEADERS потоку 1 відправник відкрив потік 3 і передав DATA обох потоків.",
];

export function Continuation() {
    const [interrupted, setInterrupted] = useState(false);
    const [step, setStep] = useState(validOrder.length);
    const tape = useRef<HTMLDivElement>(null);
    const previousPositions = useRef(new Map<Element, DOMRect>());
    const order = interrupted ? interruptedOrder : validOrder;
    const stop = interrupted ? order.indexOf(nextRequest) + 1 : order.length;
    const failed = interrupted && step === stop;
    const buffer = order.slice(0, step).filter(frame => frame.stream === 1 && frame.kind !== "DATA");

    function selectOrder(index: number) {
        const next = index === 1;
        if (next === interrupted) return;
        for (const card of tape.current?.children ?? []) {
            previousPositions.current.set(card, card.getBoundingClientRect());
        }
        setInterrupted(next);
        setStep(next ? interruptedOrder.indexOf(nextRequest) + 1 : validOrder.length);
    }

    useLayoutEffect(() => {
        const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        for (const [card, before] of previousPositions.current) {
            card.getAnimations().forEach(animation => animation.cancel());
            const after = card.getBoundingClientRect();
            const dx = before.left - after.left;
            const dy = before.top - after.top;
            if (!reducedMotion && (dx || dy)) {
                card.animate(
                    [{ transform: `translate(${dx}px, ${dy}px)` }, { transform: "none" }],
                    { duration: 580, easing: "cubic-bezier(.22,.7,.22,1)" },
                );
            }
        }
        previousPositions.current.clear();
    }, [interrupted]);

    return (
        <div className="protocol-panel continuation-demo">
            <div className="protocol-head">
                <strong>До END_HEADERS усі фрагменти йдуть поспіль</strong>
                <span>Клієнт → сервер · одне з’єднання</span>
            </div>
            <Choices
                name="Послідовність кадрів"
                labels={["Правильний порядок", "Перервати блок"]}
                selected={Number(interrupted)}
                onSelect={selectOrder}
            />
            <div className="frame-tape continuation-tape" ref={tape}>
                {order.map((frame, i) => (
                    <button
                        key={frame.bytes}
                        data-lane={frame.stream}
                        className={failed && i + 1 === stop ? "frame-error" : undefined}
                        aria-pressed={step === i + 1}
                        disabled={i >= stop}
                        onClick={() => setStep(i + 1)}
                    >
                        <small>Потік {frame.stream}</small>
                        <b>{frame.kind}</b>
                        <code>{frame.bytes}</code>
                        <small>{frame.note}</small>
                    </button>
                ))}
            </div>
            <div className="protocol-readout">
                <b>Блок HPACK · потік 1</b>
                <code>{buffer.map(frame => frame.bytes).join("  |  ") || "∅"}</code>
            </div>
            <div className="decode-state" data-failed={failed} role="status">
                {failed ? (
                    <><strong>PROTOCOL_ERROR</strong> · Сервер отримав HEADERS потоку 3 до END_HEADERS потоку 1 і закриває з’єднання.</>
                ) : receiverNotes[step]}
            </div>
            <Steps step={step} last={stop} onStep={setStep} next="Прийняти кадр" />
        </div>
    );
}

export function HttpTwoState() {
    const [streams, setStreams] = useState(2);
    const [entries, setEntries] = useState(1);
    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>Стан одного HTTP/2-з’єднання</strong>
                <span>Модель обліку · приклад без вимірювання RAM</span>
            </div>
            <div className="state-layers">
                <section>
                    <h2>Спільне для з’єднання</h2>
                    <p>TCP · парсер кадрів · вікно з’єднання</p>
                    <h3>HPACK · таблиці для двох напрямків</h3>
                    <Choices
                        name="Записи HPACK"
                        labels={["0 записів", "1 запис", "2 записи"]}
                        selected={entries}
                        onSelect={setEntries}
                    />
                    <div className="table-register">
                        <b>
                            Запити · {entries ? (entries === 1 ? 57 : 95) : 0} /
                            4096 Б
                        </b>
                        {entries === 2 && <code>#62 · x-id = 42 · 38 Б</code>}
                        {entries > 0 && (
                            <code>
                                #{entries === 2 ? 63 : 62} · :authority =
                                www.example.com · 57 Б
                            </code>
                        )}
                    </div>
                    <small>Відповіді · порожня таблиця · 0 / 4096 Б</small>
                </section>
                <section>
                    <h2>Для кожного активного потоку</h2>
                    <p>ID · стан · вікна · буфер за потреби</p>
                    <Choices
                        name="Кількість потоків"
                        labels={["0", "1", "2", "3", "4"]}
                        selected={streams}
                        onSelect={setStreams}
                    />
                    <div className="stream-registers">
                        {Array.from({ length: streams }, (_, i) => (
                            <div key={i}>
                                <b>#{2 * i + 1}</b>
                                <code>open</code>
                                <span>вікно 65 535 Б</span>
                            </div>
                        ))}
                        {!streams && <small>Немає відкритих потоків</small>}
                    </div>
                </section>
            </div>
            <p className="protocol-caption">
                Вікно — дозвіл передавати байти. Місткість HPACK — сума довжин
                імені, значення й 32 Б на запис.
            </p>
        </div>
    );
}
