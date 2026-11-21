import { useState } from "react";
import type { ReactNode } from "react";

const domain = "www.example.com";
const controls: Record<string, string> = { " ": "␠", "\r": "CR", "\n": "LF" };
const entrySize = ":authority".length + domain.length + 32;
const plainRequest = "GET / HTTP/1.1\r\nHost: " + domain + "\r\n\r\n";
const indexedFields = [
    { field: "method", label: ":method = GET", byte: 0x82 },
    { field: "scheme", label: ":scheme = http", byte: 0x86 },
    { field: "path", label: ":path = /", byte: 0x84 },
] as const;

type Field = "method" | "scheme" | "path" | "host";
type Request = "first" | "repeat";

const fieldDetails = {
    method: [
        "GET займає один байт",
        "82 означає «візьми запис №2». У вбудованій таблиці клієнта й сервера під цим номером уже є :method = GET.",
    ],
    scheme: [
        "86 означає http",
        "У рядку GET / HTTP/1.1 схема відсутня. Браузер знає її з адреси http://www.example.com/. У HTTP/2 він передає поле :scheme = http одним байтом, посилаючись на запис №6.",
    ],
    path: [
        "84 означає шлях /",
        "У тексті шлях стоїть одразу після GET. У HPACK байт 84 вибирає запис №4 у вбудованій таблиці. Сервер отримує :path = /.",
    ],
};
const hostDetails = {
    first: [
        "Першого разу домен іще треба передати",
        "41 означає «це поле :authority, запам’ятай його зі значенням». 0F задає довжину, 15 байтів. Перші три байти домену (77 77 77) кодують літери www. Клієнт і сервер записують пару під №62.",
    ],
    repeat: [
        "Тепер увесь домен займає один байт",
        `У першому запиті обидві сторони зберегли :authority = ${domain} під №62. Тепер BE означає «візьми пару №62». Ліворуч браузер знову передає весь рядок Host.`,
    ],
};

function HexByte({ label, value }: { label: string; value: number }) {
    return (
        <span className="hp-unit">
            <small>{label}</small>
            <code>{value.toString(16).padStart(2, "0").toUpperCase()}</code>
        </span>
    );
}

function LiteralBytes({ text }: { text: string }) {
    return Array.from(text, (char, index) => (
        <HexByte
            key={index}
            label={controls[char] || char}
            value={char.charCodeAt(0)}
        />
    ));
}

function PlainBytes({ text }: { text: string }) {
    return (
        <span className="hp-field">
            <LiteralBytes text={text} />
        </span>
    );
}

type FieldButtonProps = {
    field: Field;
    selected: Field;
    onSelect: (field: Field) => void;
    children: ReactNode;
};

function FieldButton({
    field,
    selected,
    onSelect,
    children,
}: FieldButtonProps) {
    return (
        <button
            type="button"
            className="hp-field"
            aria-pressed={field === selected}
            onClick={() => onSelect(field)}
        >
            {children}
        </button>
    );
}

export function HpackDemo() {
    const [request, setRequest] = useState<Request>("first");
    const [selected, onSelect] = useState<Field>("host");
    const repeat = request === "repeat";
    const count = indexedFields.length + (repeat ? 1 : 2 + domain.length);
    const [title, description] =
        selected === "host" ? hostDetails[request] : fieldDetails[selected];
    const selection = { selected, onSelect };

    function changeRequest(mode: Request) {
        setRequest(mode);
        onSelect("host");
    }

    return (
        <div className="hp-model" data-selected={selected}>
            <div
                className="switches hp-controls"
                role="group"
                aria-label="Запит у тому самому з’єднанні"
            >
                <button
                    type="button"
                    aria-pressed={!repeat}
                    onClick={() => changeRequest("first")}
                >
                    Перший запит
                </button>
                <button
                    type="button"
                    aria-pressed={repeat}
                    onClick={() => changeRequest("repeat")}
                >
                    Повторний запит
                </button>
            </div>
            <div className="hp-compare">
                <div className="hp-pane">
                    <header>
                        <h2>HTTP/1.1</h2>
                        <span className="hp-size">
                            HEX · {plainRequest.length} байт
                        </span>
                    </header>
                    <div className="hp-line">
                        <FieldButton field="method" {...selection}>
                            <LiteralBytes text="GET" />
                        </FieldButton>
                        <PlainBytes text=" " />
                        <FieldButton field="path" {...selection}>
                            <LiteralBytes text="/" />
                        </FieldButton>
                        <PlainBytes text=" " />
                        <PlainBytes text={"HTTP/1.1\r\n"} />
                    </div>
                    <div className="hp-line">
                        <FieldButton field="host" {...selection}>
                            <LiteralBytes text={"Host: " + domain} />
                        </FieldButton>
                        <PlainBytes text={"\r\n\r\n"} />
                    </div>
                </div>
                <div className="hp-pane hp-packed">
                    <header>
                        <h2>HTTP/2 · HPACK</h2>
                        <span className="hp-size">
                            HEX · {count} {repeat ? "байти" : "байтів"}
                        </span>
                    </header>
                    <div className="hp-line">
                        {indexedFields.map(({ field, label, byte }) => (
                            <FieldButton
                                key={field}
                                field={field}
                                {...selection}
                            >
                                <HexByte label={label} value={byte} />
                            </FieldButton>
                        ))}
                    </div>
                    <div className="hp-line">
                        <FieldButton field="host" {...selection}>
                            {repeat ? (
                                <HexByte
                                    label={":authority = " + domain}
                                    value={0xbe}
                                />
                            ) : (
                                <>
                                    <HexByte
                                        label="зберегти :authority"
                                        value={0x41}
                                    />
                                    <HexByte
                                        label={domain.length + " байтів"}
                                        value={domain.length}
                                    />
                                    <LiteralBytes text={domain} />
                                </>
                            )}
                        </FieldButton>
                    </div>
                </div>
            </div>
            <p className="hp-count-note">
                Динамічна таблиця HPACK ·{" "}
                <strong>
                    {repeat ? "" : "0 → "}
                    {entrySize} байтів
                </strong>{" "}
                за обліком місткості
            </p>
            <p className="hp-result" role="status">
                <strong>{title}</strong>
                <span>{description}</span>
            </p>
            <p className="hp-count-note">
                ␠ позначає пробіл (20), а CR LF позначають кінець рядка (0D 0A).
                Ліворуч показано весь запит до кінця заголовків. Праворуч
                показано блок HPACK усередині кадру HEADERS.
            </p>
        </div>
    );
}
