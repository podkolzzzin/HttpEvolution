import { useRef, useState } from "react";
import "./WindowCredits.css";

type ControlFrame = "SETTINGS" | "WINDOW_UPDATE";

const wireFields = {
    SETTINGS: [
        ["00 00 06", "Довжина тіла кадру · 6 байтів"],
        ["04", "Тип кадру · SETTINGS"],
        ["00", "Прапорці"],
        ["00 00 00 00", "ID 0 · налаштування з’єднання"],
        ["00 04", "Параметр SETTINGS_INITIAL_WINDOW_SIZE"],
        ["00 00 00 08", "Початкове вікно кожного потоку · 8 байтів"],
    ],
    WINDOW_UPDATE: [
        ["00 00 04", "Довжина тіла кадру · 4 байти"],
        ["08", "Тип кадру · WINDOW_UPDATE"],
        ["00", "Прапорці"],
        ["00 00 00 01", "ID 1 · оновити вікно потоку №1"],
        ["00 00 00 04", "Додати 4 байти до залишку вікна"],
    ],
} as const;

type MessageProps = {
    leftward: boolean;
    text: string;
    frame: string;
    inspect?: () => void;
};

function Message({ leftward, text, frame, inspect }: MessageProps) {
    return (
        <li className="wf-message" data-leftward={leftward} data-control={!!inspect}>
            <span className="wf-sr">{leftward ? "Сервер до браузера." : "Браузер до сервера."}</span>
            <strong>{text}</strong>
            <div className="wf-label">
                <code>{frame}</code>
                {inspect && <button type="button" aria-label={`Показати байти кадру ${frame}`} onClick={inspect}>Байти</button>}
            </div>
            <span className="wf-arrow" aria-hidden="true" />
        </li>
    );
}

export default function WindowCredits() {
    const [upload, setUpload] = useState(false);
    const [frame, setFrame] = useState<ControlFrame>("SETTINGS");
    const dialog = useRef<HTMLDialogElement>(null);
    const sender = upload ? "Браузер" : "Сервер";
    const receiver = upload ? "Сервер" : "Браузер";

    function inspect(selected: ControlFrame) {
        setFrame(selected);
        dialog.current?.showModal();
    }

    return (
        <div className="wf-demo">
            <div className="wf-modes" role="group" aria-label="Напрямок передачі файлу">
                <button type="button" aria-pressed={!upload} onClick={() => setUpload(false)}>Файл до браузера</button>
                <button type="button" aria-pressed={upload} onClick={() => setUpload(true)}>Файл на сервер</button>
            </div>
            <figure className="wf-timeline">
                <figcaption>{receiver} не встигає записувати файл на диск.</figcaption>
                <div className="wf-peers">
                    <b>Браузер</b><span>Файл · 12 байтів</span><b>Сервер</b>
                </div>
                <ol className="wf-events">
                    <Message leftward={upload} text="«Прийму по 8 байтів на потік»"
                        frame="SETTINGS_INITIAL_WINDOW_SIZE = 8" inspect={() => inspect("SETTINGS")} />
                    <Message leftward={!upload} text="Перші 8 байтів файлу" frame="DATA · потік №1" />
                    <li className="wf-pause">
                        <span><b>{receiver}</b> пише на диск.</span>
                        <span><b>{sender}</b> чекає. Вікно — 0.</span>
                    </li>
                    <Message leftward={upload} text="«Дозволяю ще 4 байти»"
                        frame="WINDOW_UPDATE · ID 1 · +4" inspect={() => inspect("WINDOW_UPDATE")} />
                    <Message leftward={!upload} text="Останні 4 байти файлу" frame="DATA · потік №1 · END_STREAM" />
                </ol>
            </figure>
            <dialog ref={dialog} className="wf-inspector" aria-labelledby="wf-frame-title">
                <div className="wf-dialog-head">
                    <h2 id="wf-frame-title">{frame}</h2>
                    <form method="dialog"><button type="submit">Закрити</button></form>
                </div>
                <p>{receiver} → {sender} · {frame === "SETTINGS" ? "15" : "13"} байтів</p>
                <dl>
                    {wireFields[frame].map(([bytes, meaning], index) => (
                        <div key={meaning} className={index === 4 ? "wf-payload" : undefined}>
                            <dt><code>{bytes}</code></dt><dd>{meaning}</dd>
                        </div>
                    ))}
                </dl>
                <p>{frame === "SETTINGS"
                    ? "Початкове вікно за замовчуванням — 65 535 байтів. Тут одержувач задає 8 для кожного потоку. Вікно з’єднання рахується окремо."
                    : "У потоці №1 залишок зростає з 0 до 4 байтів. Для поповнення вікна всього з’єднання одержувач надіслав би окремий кадр з ID 0."}</p>
            </dialog>
        </div>
    );
}
