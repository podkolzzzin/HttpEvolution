export function Choices({
    labels,
    selected,
    onSelect,
    name,
}: {
    labels: readonly string[];
    selected: number;
    onSelect: (index: number) => void;
    name: string;
}) {
    return (
        <div
            className="switches protocol-choices"
            role="group"
            aria-label={name}
        >
            {labels.map((label, i) => (
                <button
                    key={label}
                    type="button"
                    aria-pressed={selected === i}
                    onClick={() => onSelect(i)}
                >
                    {label}
                </button>
            ))}
        </div>
    );
}

export function Steps({
    step,
    last,
    onStep,
    next = "Наступний крок",
}: {
    step: number;
    last: number;
    onStep: (step: number) => void;
    next?: string;
}) {
    return (
        <div className="protocol-steps">
            <button disabled={step === 0} onClick={() => onStep(step - 1)}>
                ← Крок назад
            </button>
            <span>
                {step} / {last}
            </span>
            <button onClick={() => onStep(step === last ? 0 : step + 1)}>
                {step === last ? "Спочатку ↺" : next + " →"}
            </button>
        </div>
    );
}
