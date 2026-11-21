import { useState } from "react";

type Packet = readonly [
    name: string,
    state?: "loss" | "blocked" | "independent",
];

function PacketLane({
    name,
    packets,
}: {
    name: string;
    packets: readonly Packet[];
}) {
    return (
        <div className="lane">
            <span>{name}</span>
            <div className="packets">
                {packets.map(([label, state]) => (
                    <span
                        className={state ? `packet ${state}` : "packet"}
                        key={label}
                    >
                        {label}
                    </span>
                ))}
            </div>
        </div>
    );
}

export function PacketLossDemo() {
    const [lost, setLost] = useState(false);
    return (
        <div
            className={lost ? "model loss-model lost-mode" : "model loss-model"}
        >
            <div className="model-label">ІНТЕРАКТИВНА СХЕМА</div>
            <button
                type="button"
                className="loss-button"
                aria-pressed={lost}
                onClick={() => setLost(!lost)}
            >
                <span>{lost ? "Відновити пакет A₁" : "Загубити пакет A₁"}</span>
                <span aria-hidden="true">↯</span>
            </button>
            <div className="lane-title">HTTP/2 · спільний порядок TCP</div>
            <PacketLane
                name="TCP"
                packets={[
                    ["A₀"],
                    ["A₁", "loss"],
                    ["B₀", "blocked"],
                    ["B₁", "blocked"],
                ]}
            />
            <div className="lane-title">
                HTTP/3 · порядок у кожному QUIC-потоці
            </div>
            <PacketLane
                name="Потік A"
                packets={[
                    ["A₀"],
                    ["A₁", "loss"],
                    ["A₂", "blocked"],
                    ["A₃", "blocked"],
                ]}
            />
            <PacketLane
                name="Потік B"
                packets={[
                    ["B₀", "independent"],
                    ["B₁", "independent"],
                    ["B₂", "independent"],
                    ["B₃", "independent"],
                ]}
            />
            <p className="model-caption" role="status">
                {lost
                    ? "A₁ втрачено. TCP чекає пропуск і затримує B. QUIC чекає лише в A; дані B можуть бути прочитані."
                    : "Усі пакети надійшли. Обидва протоколи можуть передати дані застосунку."}
            </p>
        </div>
    );
}
