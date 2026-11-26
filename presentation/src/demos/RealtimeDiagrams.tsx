import { useState } from "react";
import { Choices, Steps } from "./ProtocolControls";

export function RealtimeYears() {
    const rows = [
        ["2005 · приклад AJAX", "Polling", "Запит за таймером"],
        ["2009 · чернетка IETF", "Long Polling", "Запит до появи події"],
        ["2009 · чернетка", "SSE", "Потік подій до браузера"],
        ["2011 · RFC", "WebSocket", "Кадри в обидва боки"],
        ["2021 · стандарт", "WebRTC", "Медіа й дані між браузерами"],
    ];
    return (
        <div className="protocol-panel">
            <div className="year-rows">
                {rows.map(([year, name, idea]) => (
                    <div key={name}>
                        <time>{year}</time>
                        <strong>{name}</strong>
                        <span>{idea}</span>
                    </div>
                ))}
            </div>
        </div>
    );
}

export function WebSocketChatDiagram() {
    const [step, setStep] = useState(0);
    const lanes = [
        ["Оля ↔ сервер ↔ Марко", "Два WebSocket після HTTP Upgrade"],
        ["Оля → сервер", "TEXT · «Привіт!»"],
        ["Сервер → Марко", "TEXT · «Привіт!»"],
        ["Марко → сервер", "TEXT · «Вітаю!»"],
        ["Сервер → Оля", "TEXT · «Вітаю!»"],
    ];
    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>Один двобічний канал на клієнта</strong>
                <span>Модель чату</span>
            </div>
            <div className="chat-peers">
                <div className="peer">
                    <b>Оля</b>
                    <div>
                        {step >= 1 && <span>Привіт!</span>}
                        {step >= 4 && <span className="received">Вітаю!</span>}
                    </div>
                </div>
                <div className="chat-hop">
                    <b>{step === 0 ? "↔" : step < 3 ? "→" : "←"}</b>
                    <small>Сервер чату</small>
                </div>
                <div className="peer">
                    <b>Марко</b>
                    <div>
                        {step >= 2 && <span className="received">Привіт!</span>}
                        {step >= 3 && <span>Вітаю!</span>}
                    </div>
                </div>
            </div>
            <div className="protocol-readout" aria-live="polite">
                <b>{lanes[step][0]}</b>
                <code>{lanes[step][1]}</code>
            </div>
            <Steps
                step={step}
                last={4}
                onStep={setStep}
                next={
                    ["Надіслати", "Доставити", "Відповісти", "Доставити"][step]
                }
            />
        </div>
    );
}

export function SocketBytes() {
    const [direction, setDirection] = useState(0);
    const masked = direction === 1;
    const payload = masked
        ? ["7F", "9F", "4D", "51", "58"]
        : ["48", "65", "6C", "6C", "6F"];
    return (
        <div className="protocol-panel ws-bytes">
            <Choices
                name="Напрямок кадру"
                labels={["Сервер → браузер", "Браузер → сервер"]}
                selected={direction}
                onSelect={setDirection}
            />
            <div className="byte-groups">
                <div>
                    <small>Байт 0 · FIN / RSV / TEXT</small>
                    <strong>81</strong>
                    <code>1 · 000 · 0001</code>
                </div>
                <div>
                    <small>Байт 1 · MASK / довжина</small>
                    <strong>{masked ? "85" : "05"}</strong>
                    <code>{masked ? "1" : "0"} · 0000101</code>
                </div>
            </div>
            {masked && (
                <div className="protocol-readout">
                    <b>Байти 2–5 · приклад маски</b>
                    <code>37 FA 21 3D</code>
                </div>
            )}
            <h2>
                Байти {masked ? "6–10" : "2–6"} ·{" "}
                {masked ? "дані XOR маска" : "дані UTF-8"}
            </h2>
            <div className="wire-bytes">
                {payload.map((byte, i) => (
                    <span key={i}>
                        <b>{byte}</b>
                        <small>{"Hello"[i]}</small>
                    </span>
                ))}
            </div>
            <p className="protocol-caption">
                Довжина 0–125 у заголовку · 126 → ще 2 байти · 127 → ще 8 байтів
            </p>
        </div>
    );
}

export function WebSocketSecurity() {
    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>wss://chat.example</strong>
                <span>WebSocket через TLS · HTTP/1.1</span>
            </div>
            <ol className="protocol-ledger">
                <li>
                    <b>TLS handshake</b>
                    <span>
                        Перевірка сертифіката й імені сервера → захищений канал
                    </span>
                </li>
                <li>
                    <b>HTTP Upgrade</b>
                    <span>
                        Запит усередині TLS · перевірка Origin для браузерів і
                        сесії користувача
                    </span>
                </li>
                <li>
                    <b>101 → кадри</b>
                    <span>
                        TLS → конфіденційність та цілісність заголовків кадрів і
                        даних
                    </span>
                </li>
                <li>
                    <b>Маска клієнта</b>
                    <span>
                        Захист проміжних HTTP-вузлів · ключ маски в самому кадрі
                    </span>
                </li>
                <li>
                    <b>На сервері</b>
                    <span>
                        Доступ до кімнати · перевірка повідомлень · ліміт
                        розміру
                    </span>
                </li>
            </ol>
        </div>
    );
}
