import { useState } from "react";
import data from "../data/serverMemory.json";

const protocols = [
    ["http11", "HTTP/1.1"],
    ["http11-tls", "HTTP/1.1 + TLS 1.3"],
    ["h2", "HTTP/2 (h2c)"],
    ["h2-tls", "HTTP/2 + TLS 1.3"],
    ["h3", "HTTP/3 (QUIC)"],
] as const;
const samples = [
    ["warm", "Прогрітий"],
    ["64", "64"],
    ["256", "256"],
    ["512", "512"],
    ["closed", "Після закриття"],
] as const;
const controls = [
    ["hpack", "HPACK · індексація заголовка на 4 000 ASCII-символів"],
    ["quicStreams", "HTTP/3 · 8 завершених потоків замість одного"],
] as const;
const servers = ["kestrel", "nginx"] as const;
const signedMiB = new Intl.NumberFormat("uk", {
    minimumFractionDigits: 1,
    maximumFractionDigits: 2,
    signDisplay: "exceptZero",
});
const unsignedMiB = new Intl.NumberFormat("uk", {
    minimumFractionDigits: 1,
    maximumFractionDigits: 2,
});

function formatMemory(kib: number) {
    return signedMiB.format(kib / 1024);
}

export function ServerMemoryDemo() {
    const [sample, setSample] = useState<(typeof samples)[number][0]>("512");
    return (
        <div className="memory-benchmark server-memory">
            <div className="switches" role="group" aria-label="Стан з’єднань">
                {samples.map(([id, label]) => (
                    <button key={id} type="button" aria-pressed={sample === id}
                        onClick={() => setSample(id)}>
                        {label}
                    </button>
                ))}
            </div>
            <table className="memory-results server-memory-table">
                <caption>
                    {sample === "warm" ? "PSS, МіБ · прогрітий сервер без клієнтів" :
                        `Δ PSS, МіБ · ${sample === "closed"
                            ? "через 2 с після закриття 512 з’єднань"
                            : `${sample} відкритих з’єднань`}`}
                </caption>
                <thead>
                    <tr>
                        <th scope="col">Протокол</th>
                        <th scope="col">Kestrel</th>
                        <th scope="col">nginx</th>
                    </tr>
                </thead>
                <tbody>
                    {protocols.map(([id, title]) => (
                        <tr key={id}>
                            <th scope="row">{title}</th>
                            {servers.map(server => {
                                const row = data.modes[id][server];
                                const memory = sample === "warm"
                                    ? unsignedMiB.format(row.warm / 1024)
                                    : formatMemory(sample === "closed" ? row.closed : row.idle[sample]);
                                return <td key={server}>{memory}</td>;
                            })}
                        </tr>
                    ))}
                </tbody>
            </table>
            <p>Медіана {data.repeats} запусків. PSS охоплює керовані й нативні сторінки та ділить спільні сторінки між процесами. Δ показує зміну відносно прогрітого сервера.</p>
            <details className="wire memory-method">
                <summary>Умови, HPACK і потоки HTTP/3</summary>
                <div>
                    <p>Kestrel .NET 10.0.1 і nginx 1.30.7 з двома воркерами. Linux, loopback, той самий сертифікат і відповідь на 22 байти. Мережеві буфери ядра залишаються поза PSS.</p>
                    <table className="memory-results server-memory-table">
                        <caption>Різниця PSS між контрольними сценаріями при 512 з’єднаннях, МіБ</caption>
                        <thead>
                            <tr>
                                <th scope="col">Контроль</th>
                                <th scope="col">Kestrel</th>
                                <th scope="col">nginx</th>
                            </tr>
                        </thead>
                        <tbody>
                            {controls.map(([id, title]) => (
                                <tr key={id}>
                                    <th scope="row">{title}</th>
                                    {servers.map(server => {
                                        const { median, range } = data.controls[id][server];
                                        return (
                                            <td key={server}>
                                                {formatMemory(median)}
                                                <small>{range.map(formatMemory).join(" … ")}</small>
                                            </td>
                                        );
                                    })}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <p>Під медіаною наведено діапазон повторів. Контроль HPACK порівнює той самий заголовок з індексацією та без неї. Різниця PSS охоплює весь процес, зокрема стан алокаторів і буферів.</p>
                    <p>На HTTP/0.9 Kestrel повернув 400, nginx віддав HTML без заголовків. Після звичайного HTTP/1.0 без keep-alive обидва сервери закрили TCP.</p>
                </div>
            </details>
            <div className="memory-links">
                <a href="./server-memory.csv" download>Сирі вимірювання (CSV)</a>
                <a href="./server-memory-legacy.csv" download>Проби HTTP/0.9 і 1.0 (CSV)</a>
            </div>
        </div>
    );
}
