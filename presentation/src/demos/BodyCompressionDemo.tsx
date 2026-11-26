import { useEffect, useState } from "react";
import samples from "../data/body-compression.json";
import { Choices } from "./ProtocolControls";
import "./BodyCompressionDemo.css";

type Mode = "gzip" | "modern";
type Coding = keyof typeof samples.html.sizes;
const resources = [
    { key: "html", label: "Список HTML", type: "text/html", path: "/photos.html" },
    { key: "ok", label: "Коротке «OK»", type: "text/plain", path: "/status" },
    { key: "jpeg", label: "Фото JPEG", type: "image/jpeg", path: "/earth.jpg" },
] as const;
const codingLabels = { identity: "Без стискання", gzip: "gzip", br: "br", zstd: "zstd" };
const number = new Intl.NumberFormat("uk-UA");

type Variant = { coding: Coding; bytes: number };
type ExchangeProps = {
    variants: Variant[];
    selected: Coding;
    onSelect: (coding: Coding) => void;
    mode: Mode;
    resource: typeof resources[number];
};

function Exchange({ variants, selected, onSelect, mode, resource }: ExchangeProps) {
    const original = variants[0].bytes;
    const { bytes } = variants.find(({ coding }) => coding === selected)!;
    const maximum = Math.max(...variants.map((variant) => variant.bytes));
    const legacy = mode === "gzip";
    const version = legacy ? "HTTP/1.0" : "HTTP/1.1";
    const codingToken = legacy && selected === "gzip" ? "x-gzip" : selected;
    const difference = original - bytes;
    const saving = difference === 0 ? "Початковий розмір" :
        `${number.format(Math.abs(difference))} Б ${difference > 0 ? "менше" : "більше"}`;
    const contentEncoding = selected !== "identity" && `Content-Encoding: ${codingToken}`;

    return (
        <>
            <div className="body-compression-chart" role="group" aria-label="Вибір представлення відповіді">
                {variants.map(({ coding, bytes: size }) => (
                    <button type="button" key={coding} aria-pressed={coding === selected}
                        onClick={() => onSelect(coding)}>
                        <span>{codingLabels[coding]}</span>
                        <span className="body-compression-track" aria-hidden="true">
                            <i style={{ width: `${(size / maximum) * 100}%` }} />
                        </span>
                        <strong>{number.format(size)} Б</strong>
                    </button>
                ))}
            </div>
            <div className="body-compression-wire" aria-live="polite">
                <div>
                    <small>Клієнт → сервер</small>
                    <code>GET {resource.path} {version}</code>
                    {!legacy && <code>Host: example.org</code>}
                    <code>Accept-Encoding: {legacy ? "x-gzip" : "gzip, br, zstd"}</code>
                </div>
                <div>
                    <small>Сервер → клієнт</small>
                    <code>{version} 200 OK</code>
                    <code>Content-Type: {resource.type}</code>
                    {contentEncoding && <code className="body-compression-encoding">{contentEncoding}</code>}
                    <code>Content-Length: {bytes}</code>
                    {!legacy && <code>Vary: Accept-Encoding</code>}
                </div>
            </div>
            <p className="body-compression-saving" role="status">{saving}</p>
        </>
    );
}

function ModernComparison() {
    const [selected, setSelected] = useState<Coding>("br");
    const [index, setIndex] = useState(0);
    const resource = resources[index];
    const sizes = samples[resource.key].sizes;
    const codings: Coding[] = ["identity", "gzip", "br", "zstd"];
    return (
        <>
            <Choices name="Приклад тіла відповіді" labels={resources.map(({ label }) => label)}
                selected={index} onSelect={setIndex} />
            <Exchange mode="modern" resource={resource} selected={selected} onSelect={setSelected}
                variants={codings.map((coding) => ({ coding, bytes: sizes[coding] }))} />
            <small className="body-compression-method">Виміряні зразки · gzip 6, Brotli 5, Zstandard 3 · без заголовків</small>
        </>
    );
}

type GzipResult = { source: string; bytes?: number };

function GzipComparison() {
    const [selected, setSelected] = useState<Coding>("gzip");
    const [index, setIndex] = useState(0);
    const [source, setSource] = useState(samples.html.source);
    const [result, setResult] = useState<GzipResult>();
    const preset = index === 0 ? samples.html : samples.ok;
    const custom = source !== preset.source;
    const canEdit = typeof CompressionStream === "function";

    useEffect(() => {
        if (!custom) return;
        let current = true;
        async function measure() {
            try {
                const compressed = new Blob([source]).stream().pipeThrough(new CompressionStream("gzip"));
                const body = await new Response(compressed).arrayBuffer();
                if (current) setResult({ source, bytes: body.byteLength });
            } catch {
                if (current) setResult({ source });
            }
        }
        void measure();
        return () => { current = false; };
    }, [source, custom]);

    const current = !custom ? preset.sizes.gzip : result?.source === source ? result.bytes : undefined;
    const updating = custom && result?.source !== source;
    return (
        <>
            <Choices name="Приклад тіла відповіді" labels={resources.slice(0, 2).map(({ label }) => label)}
                selected={custom ? -1 : index} onSelect={(next) => {
                    setIndex(next);
                    setSource(next === 0 ? samples.html.source : samples.ok.source);
                }} />
            {current !== undefined ? (
                <Exchange mode="gzip" resource={resources[index]} selected={selected} onSelect={setSelected} variants={[
                    { coding: "identity", bytes: new Blob([source]).size },
                    { coding: "gzip", bytes: current },
                ]} />
            ) : (
                <p role="status">{updating ? "Стискаємо…" : "Браузеру не вдалося стиснути текст. Виберіть зразок."}</p>
            )}
            {canEdit && (
                <details className="body-compression-edit">
                    <summary>Змінити тіло відповіді</summary>
                    <textarea aria-label="Тіло відповіді у UTF-8" value={source} maxLength={16000}
                        onChange={(event) => setSource(event.target.value)} spellCheck={false} rows={3} />
                </details>
            )}
        </>
    );
}

export function BodyCompressionDemo({ mode }: { mode: Mode }) {
    return (
        <div className="body-compression">
            {mode === "gzip" ? <GzipComparison /> : <ModernComparison />}
        </div>
    );
}
