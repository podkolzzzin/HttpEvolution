import { useState } from "react";

const fields = [
    {
        id: "length",
        label: "Довжина",
        bytes: "00 00 05",
        size: "3 байти",
        description:
            "5 байтів вмісту · заголовок окремо. Типовий ліміт 16 384 Б. Одержувач задає SETTINGS_MAX_FRAME_SIZE від 16 384 до 16 777 215 Б (24 біти).",
    },
    {
        id: "type",
        label: "Тип",
        bytes: "00",
        size: "1 байт",
        description:
            "Тип: 00 означає DATA — кадр несе шматок тіла відповіді. Тип 01 означав би HEADERS — HTTP-заголовки.",
    },
    {
        id: "flags",
        label: "Прапорці",
        bytes: "01",
        size: "1 байт",
        description:
            "Прапорці: 01 вмикає END_STREAM. Для нашого DATA це означає: «Це останній шматок відповіді в цьому потоці». TCP-з’єднання залишається відкритим.",
    },
    {
        id: "stream",
        label: "Потік",
        bytes: "00 00 00 01",
        size: "4 байти",
        description:
            "Потік: 00 00 00 01 означає №1 — кадр належить цьому діалогу. У 4 байтах старший біт зарезервований. Про кілька діалогів — наступний слайд.",
    },
];

export function FrameDemo() {
    const [selected, select] = useState(fields[0]);
    return (
        <div className="frame-model" aria-label="Приклад кадру HTTP/2">
            <h2>Спочатку опис, потім вміст</h2>
            <p className="frame-hint">
                Це 9 службових байтів у hex. Натисни на поле.
            </p>
            <div className="frame-fields">
                {fields.map((field) => (
                    <button
                        className="frame-field"
                        type="button"
                        key={field.id}
                        aria-pressed={field === selected}
                        onClick={() => select(field)}
                    >
                        <strong>{field.label}</strong>
                        <code>{field.bytes}</code>
                        <small>{field.size}</small>
                    </button>
                ))}
            </div>
            <p className="frame-explanation" role="status">
                {selected.description}
            </p>
            {selected.id === "type" && (
                <dl className="frame-catalog" aria-label="Базові типи HTTP/2">
                    {[
                        ["00 · DATA", "Тіло"],
                        ["01 · HEADERS", "Заголовки"],
                        ["02 · PRIORITY", "Пріоритет (застарілий)"],
                        ["03 · RST_STREAM", "Скидання потоку"],
                        ["04 · SETTINGS", "Параметри"],
                        ["05 · PUSH_PROMISE", "Обіцяний потік"],
                        ["06 · PING", "Перевірка зв’язку"],
                        ["07 · GOAWAY", "Завершення з’єднання"],
                        ["08 · WINDOW_UPDATE", "Дозвіл на DATA"],
                        ["09 · CONTINUATION", "Продовження заголовків"],
                    ].map(([name, meaning]) => (
                        <div key={name}>
                            <dt>{name}</dt>
                            <dd>{meaning}</dd>
                        </div>
                    ))}
                </dl>
            )}
            {selected.id === "flags" && (
                <dl
                    className="frame-catalog"
                    aria-label="Базові прапорці за типом кадру"
                >
                    {[
                        ["01 · END_STREAM", "DATA, HEADERS"],
                        ["01 · ACK", "SETTINGS, PING"],
                        [
                            "04 · END_HEADERS",
                            "HEADERS, PUSH_PROMISE, CONTINUATION",
                        ],
                        ["08 · PADDED", "DATA, HEADERS, PUSH_PROMISE"],
                        ["20 · PRIORITY", "HEADERS (застарілий)"],
                    ].map(([name, types]) => (
                        <div key={name}>
                            <dt>{name}</dt>
                            <dd>{types}</dd>
                        </div>
                    ))}
                </dl>
            )}
            <div className="frame-payload">
                <span>Дані · 5 байтів ASCII</span>
                <strong>Hello</strong>
            </div>
            <div className="frame-total">
                3 + 1 + 1 + 4 = 9 · увесь кадр: 9 + 5 = 14 байтів
            </div>
        </div>
    );
}
