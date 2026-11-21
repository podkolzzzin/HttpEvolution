import { useState } from "react";

const requests = [
    { label: "Є Host", response: "200 OK · Host: localhost" },
    { label: "Немає", response: "400 Bad Request · Host відсутній" },
    { label: "Дублікат", response: "400 Bad Request · два заголовки Host" },
];

export function HostValidationDemo() {
    const [request, select] = useState(requests[0]);
    return (
        <div className="model" aria-label="Модель перевірки Host">
            <div className="model-label">СПРОБУЙ · HOST У HTTP/1.1</div>
            <div className="switches">
                {requests.map((option) => (
                    <button
                        type="button"
                        key={option.label}
                        aria-pressed={option === request}
                        onClick={() => select(option)}
                    >
                        {option.label}
                    </button>
                ))}
            </div>
            <p
                className={
                    request === requests[0] ? "host-result" : "host-result bad"
                }
                role="status"
            >
                {request.response}
            </p>
        </div>
    );
}
