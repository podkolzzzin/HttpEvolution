import type { Channel } from "../data/types";

export function ChannelDiagram({ channel }: { channel: Channel }) {
    const sse = channel === "sse";
    return (
        <div className="channel-model">
            <div className="channel-path">
                <strong>Браузер</strong>
                <span>{sse ? "← події" : "↔ повідомлення"}</span>
                <strong>Сервер</strong>
            </div>
            <p>
                {sse
                    ? "SSE: одне HTTP-з’єднання для вхідних подій."
                    : "WebSocket: кожна сторона надсилає, коли має дані."}
            </p>
        </div>
    );
}
