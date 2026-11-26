import { useState } from "react";
import loaded from "../data/serverMemory.json";
import single from "../data/singleServerMemory.json";

const loads = [0, 1, 64, 256, 512] as const;
const protocols = [
    ["http11", "HTTP/1.1"],
    ["http11-tls", "HTTP/1.1 + TLS 1.3"],
    ["h2", "HTTP/2 (h2c)"],
    ["h2-tls", "HTTP/2 + TLS 1.3"],
    ["h3", "HTTP/3 (QUIC)"],
] as const;
const servers = [["demo", "Наш сервер"], ["nginx", "nginx"], ["kestrel", "Kestrel"]] as const;
const mib = new Intl.NumberFormat("uk", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

function readPss(
    protocol: (typeof protocols)[number][0],
    server: (typeof servers)[number][0],
    load: (typeof loads)[number],
) {
    if (load === 0 || load === 1) {
        return single.modes[protocol][server][load === 0 ? "warm" : "idle"].pss;
    }
    if (server === "demo") return null;
    return loaded.modes[protocol][server].idlePss[load];
}

export function ServerMemoryDemo() {
    const [load, setLoad] = useState<(typeof loads)[number]>(1);

    const rows = protocols.map(([id, title]) => ({
        id,
        title,
        readings: servers.map(([server]) => ({ server, kib: readPss(id, server, load) })),
    }));
    const maxKiB = Math.max(...rows.flatMap(row => row.readings.map(({ kib }) => kib ?? 0)));

    return (
        <div className="memory-benchmark">
            <fieldset className="sample-picker">
                <legend>Відкриті з’єднання</legend>
                <div className="switches">
                    {loads.map(count => (
                        <button key={count} type="button" aria-pressed={load === count}
                            onClick={() => setLoad(count)}>
                            {count}
                        </button>
                    ))}
                </div>
            </fieldset>
            <table className="memory-results">
                <caption>
                    Пам’ять процесів (PSS), МіБ
                    <small>100% для цього навантаження — {mib.format(maxKiB / 1024)} МіБ</small>
                </caption>
                <thead>
                    <tr>
                        <th scope="col">Протокол</th>
                        {servers.map(([id, title]) => <th scope="col" key={id} data-server={id}>{title}</th>)}
                    </tr>
                </thead>
                <tbody>
                    {rows.map(({ id, title, readings }) => (
                        <tr key={id}>
                            <th scope="row">{title}</th>
                            {readings.map(({ server, kib }) => {
                                if (kib === null) return <td key={server}>—</td>;
                                const value = kib / 1024;
                                return (
                                    <td key={server} data-server={server}>
                                        {mib.format(value)}
                                        <span className="pss-track" aria-hidden="true">
                                            <span style={{ width: `${maxKiB > 0 ? 100 * kib / maxKiB : 0}%` }} />
                                        </span>
                                    </td>
                                );
                            })}
                        </tr>
                    ))}
                </tbody>
            </table>
            {load > 1 && (
                <p>Прочерк означає, що навантаження перевищує ліміт нашого демо. Воно обслуговує одного TCP-клієнта або до 32 одночасних QUIC-сесій.</p>
            )}
            <a href={load > 1 ? "./server-memory.csv" : "./single-server-memory.csv"} download>
                Сирі виміри (CSV)
            </a>
        </div>
    );
}
