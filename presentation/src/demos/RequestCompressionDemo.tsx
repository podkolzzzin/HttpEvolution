import { useEffect, useState } from "react";
import "./RequestCompressionDemo.css";

const events = Array.from({ length: 24 }, (_, index) => ({
    id: index + 1,
    event: "перегляд",
}));
const json = new Blob([JSON.stringify(events)], { type: "application/json" });

export function RequestCompressionDemo() {
    const [acceptsGzip, setAcceptsGzip] = useState(true);
    const [sendGzip, setSendGzip] = useState(true);
    const [compressed, setCompressed] = useState<Uint8Array>();
    const [compressionFailed, setCompressionFailed] = useState(false);

    useEffect(() => {
        let active = true;
        async function prepareBody() {
            try {
                const stream = json.stream().pipeThrough(
                    new CompressionStream("gzip"),
                );
                const bytes = new Uint8Array(
                    await new Response(stream).arrayBuffer(),
                );
                if (active) setCompressed(bytes);
            } catch {
                if (active) {
                    setCompressionFailed(true);
                    setSendGzip(false);
                }
            }
        }
        void prepareBody();
        return () => {
            active = false;
        };
    }, []);

    const rejected = sendGzip && !acceptsGzip;
    const bodySize = sendGzip ? compressed?.byteLength : json.size;
    const gzipPrefix = compressed && Array.from(
        compressed.subarray(0, 8),
        (byte) => byte.toString(16).padStart(2, "0"),
    ).join(" ");
    const prefix = sendGzip ? gzipPrefix : "[" + JSON.stringify(events[0]) + ",";
    const sendLabel = sendGzip
        ? "Надіслати без стискання"
        : "Надіслати з gzip";
    const nextAction = rejected ? "Повторити без стискання" : sendLabel;

    return (
        <section className="protocol-panel upload-compression" aria-label="Модель стисненого POST-запиту">
            <div className="upload-settings">
                <span><strong>{events.length} події</strong> у JSON · {json.size} Б</span>
                <label>
                    <input
                        type="checkbox"
                        checked={acceptsGzip}
                        onChange={(event) => setAcceptsGzip(event.target.checked)}
                    />
                    Сервер приймає gzip
                </label>
            </div>
            <div className="upload-exchange" role="status" aria-atomic="true">
                <div className="upload-request">
                    <h2>Клієнт → сервер</h2>
                    <pre>{[
                        "POST /events HTTP/1.1",
                        "Host: demo.test",
                        "Content-Type: application/json",
                        ...(sendGzip ? ["Content-Encoding: gzip"] : []),
                        `Content-Length: ${bodySize ?? "…"}`,
                    ].join("\n")}</pre>
                    <code className="upload-prefix">{prefix} …</code>
                    <div className="upload-meter" aria-hidden="true">
                        <span style={{ width: `${100 * (bodySize ?? 0) / json.size}%` }} />
                    </div>
                    <small>Тіло на дроті · {bodySize ?? "…"} Б</small>
                </div>
                <div className={rejected ? "upload-response rejected" : "upload-response"}>
                    <h2>Сервер → клієнт</h2>
                    <div className="upload-status">{rejected ? "415" : "204"}</div>
                    <code>{rejected ? "Unsupported Media Type" : "No Content"}</code>
                    {rejected && <pre>Accept-Encoding: identity</pre>}
                    <p>{rejected
                        ? "Цей ресурс приймає лише нестиснене тіло."
                        : `Сервер прочитав усі ${events.length} події.`}</p>
                </div>
            </div>
            <div className="upload-action">
                {compressionFailed ? (
                    <small>Цей браузер не підтримує стискання gzip.</small>
                ) : (
                    <button type="button" disabled={!compressed} onClick={() => setSendGzip(!sendGzip)}>
                        {nextAction} →
                    </button>
                )}
            </div>
        </section>
    );
}
