import { useId, useState } from "react";
import { sslDescriptions } from "./sslDescriptions";
import type { TrafficFieldId } from "./sslDescriptions";

type Field = {
    id: TrafficFieldId;
    title: string;
    code: string;
    note: string;
};

const httpFields: readonly Field[] = [
    {
        id: "request",
        title: "Рядок запиту",
        code: "POST / HTTP/1.0\\r\\n",
        note: "16 байтів",
    },
    {
        id: "headers",
        title: "HTTP-заголовок + порожній рядок",
        code: "Content-Length: 3\\r\\n\\r\\n",
        note: "22 байти",
    },
    { id: "body", title: "Тіло", code: "x=1", note: "3 байти" },
];

const decodedFields: readonly Field[] = [
    { id: "http", title: "Увесь HTTP", code: "16 + 22 + 3", note: "41 байт" },
    { id: "mac", title: "MAC", code: "SHA-1", note: "20 байтів" },
    { id: "padding", title: "Доповнення", code: "02 02 02", note: "3 байти" },
];

type SelectionProps = {
    selected: TrafficFieldId;
    detailId: string;
    onSelect: (field: TrafficFieldId) => void;
};

function TrafficField({
    id,
    title,
    code,
    note,
    selected,
    detailId,
    onSelect,
}: Field & SelectionProps) {
    return (
        <button
            type="button"
            className="traffic-field"
            aria-pressed={id === selected}
            aria-controls={detailId}
            onClick={() => onSelect(id)}
        >
            <strong>{title}</strong>
            <code>{code}</code>
            <small>{note}</small>
        </button>
    );
}

function HttpRequest(props: SelectionProps) {
    return (
        <div className="traffic-http">
            {httpFields.map((field) => (
                <TrafficField key={field.id} {...field} {...props} />
            ))}
        </div>
    );
}

function Ciphertext({ selected, detailId, onSelect }: SelectionProps) {
    return (
        <div className="traffic-cipher">
            <button
                type="button"
                aria-pressed={selected === "cipher"}
                aria-controls={detailId}
                onClick={() => onSelect("cipher")}
            >
                <svg
                    viewBox="0 0 30 36"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.5"
                    aria-hidden="true"
                >
                    <rect x="4" y="15" width="22" height="18" rx="3" />
                    <path d="M9 15V9a6 6 0 0 1 12 0v6m-6 8v4" />
                </svg>
                <span>
                    <strong>Шифротекст · 64 байти</strong>
                    <small>
                        HTTP + MAC + доповнення
                        <br />
                        Вміст прихований від спостерігача
                    </small>
                </span>
            </button>
        </div>
    );
}

function SslRecord({
    revealed,
    ...selection
}: SelectionProps & { revealed: boolean }) {
    return (
        <>
            <div className="traffic-record-title">
                Дані TCP: один SSL-запис · 69 байтів
            </div>
            <div className="traffic-record-parts">
                <TrafficField
                    id="record"
                    title="Заголовок SSL"
                    code="17 03 00 00 40"
                    note="Відкрито · 5 байтів"
                    {...selection}
                />
                {revealed ? (
                    <div className="traffic-decoded">
                        <p className="traffic-decoded-label">
                            Лише на кінцевій стороні після розшифрування.
                            <br />У мережі всі ці 64 байти зашифровані разом.
                        </p>
                        <div className="traffic-decoded-parts">
                            {decodedFields.map((field) => (
                                <TrafficField
                                    key={field.id}
                                    {...field}
                                    {...selection}
                                />
                            ))}
                        </div>
                    </div>
                ) : (
                    <Ciphertext {...selection} />
                )}
            </div>
        </>
    );
}

type Props = {
    ssl: boolean;
    onModeChange: (ssl: boolean) => void;
};

export function SslTrafficDemo({ ssl, onModeChange }: Props) {
    const [revealed, setRevealed] = useState(false);
    const [selected, onSelect] = useState<TrafficFieldId>("cipher");
    const detailId = useId();
    const drawingId = useId();
    const selection = { selected, detailId, onSelect };
    const [title, body] = sslDescriptions(ssl)[selected];

    function changeMode(enabled: boolean) {
        onModeChange(enabled);
        setRevealed(false);
        onSelect(enabled ? "cipher" : "http");
    }

    function toggleReveal() {
        setRevealed(!revealed);
        onSelect(revealed ? "cipher" : "http");
    }

    return (
        <div className="traffic-model">
            <div className="traffic-controls">
                <div
                    className="switches"
                    role="group"
                    aria-label="Спосіб передавання"
                >
                    <button
                        type="button"
                        aria-pressed={!ssl}
                        onClick={() => changeMode(false)}
                    >
                        Без SSL
                    </button>
                    <button
                        type="button"
                        aria-pressed={ssl}
                        onClick={() => changeMode(true)}
                    >
                        Із SSL3
                    </button>
                </div>
                <button
                    type="button"
                    className="traffic-reveal"
                    disabled={!ssl}
                    aria-pressed={ssl && revealed}
                    aria-controls={drawingId}
                    onClick={toggleReveal}
                >
                    {!ssl
                        ? "HTTP уже відкритий"
                        : revealed
                          ? "Повернути вигляд у мережі"
                          : "Показати вміст на сервері"}
                </button>
            </div>
            <div id={drawingId}>
                <div className="traffic-caption">
                    <strong>У клієнта до передавання · HTTP, 41 байт</strong>
                    <span>{"\\r\\n = два байти кінця рядка"}</span>
                </div>
                <HttpRequest {...selection} />
                <div className="traffic-transform">
                    <svg
                        viewBox="0 0 20 22"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="1.5"
                        aria-hidden="true"
                    >
                        <path d="M10 1v18m-6-6 6 6 6-6" />
                    </svg>
                    <span>
                        {ssl
                            ? "Додати MAC і доповнення → зашифрувати 3DES-CBC"
                            : "Передати ті самі байти в TCP без шифрування"}
                    </span>
                </div>
                <div className="traffic-caption">
                    <strong>
                        {revealed && ssl
                            ? "Розбір на сервері з ключем"
                            : "У мережі · що може прочитати спостерігач"}
                    </strong>
                    <span>Порядок полів; розміри блоків на схемі умовні</span>
                </div>
                <div className="traffic-network">
                    <TrafficField
                        id="ip"
                        title="IP"
                        code="Адреси"
                        note="Відкрито"
                        {...selection}
                    />
                    <TrafficField
                        id="tcp"
                        title="TCP"
                        code="Порти"
                        note="Відкрито"
                        {...selection}
                    />
                    <div className="traffic-record">
                        {ssl ? (
                            <SslRecord revealed={revealed} {...selection} />
                        ) : (
                            <>
                                <div className="traffic-record-title">
                                    Дані TCP: відкритий HTTP · 41 байт
                                </div>
                                <HttpRequest {...selection} />
                            </>
                        )}
                    </div>
                </div>
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
            <p className="traffic-boundaries">
                У режимі SSL показано дані після handshake: SSL3 + 3DES-CBC +
                SHA-1 MAC, без стиснення. Тут запит умістився в один SSL-запис.
                Загалом TCP може розбити запис між сегментами або передати
                кілька записів разом; межі HTTP, SSL і TCP не зобов’язані
                збігатися.
            </p>
        </div>
    );
}
