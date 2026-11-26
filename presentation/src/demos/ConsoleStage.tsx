import { useEffect, useRef, useState } from "react";
import type { CSSProperties } from "react";
import { ProtocolConsole, PROTOCOL_CONSOLE_DURATION } from "./ProtocolConsole";
import "./ConsoleStage.css";

const period = PROTOCOL_CONSOLE_DURATION * 2000;

export function ConsoleStage({ onBack }: { onBack: () => void }) {
    const [playing, setPlaying] = useState(
        () => !matchMedia("(prefers-reduced-motion: reduce)").matches,
    );
    const [elapsed, setElapsed] = useState(0);
    const position = useRef(0);
    const [controls, setControls] = useState(false);
    const hideTimer = useRef<number>(undefined);

    useEffect(() => () => clearTimeout(hideTimer.current), []);
    useEffect(() => {
        if (!playing) return;
        const origin = performance.now() - position.current;
        let frame: number;
        function tick(now: number) {
            position.current = (now - origin) % period;
            setElapsed(position.current);
            frame = requestAnimationFrame(tick);
        }
        frame = requestAnimationFrame(tick);
        return () => cancelAnimationFrame(frame);
    }, [playing]);

    function reveal() {
        setControls(true);
        clearTimeout(hideTimer.current);
        hideTimer.current = window.setTimeout(() => setControls(false), 2200);
    }

    const phase = elapsed / period * 2 * Math.PI;
    const progress = (1 - Math.cos(phase)) / 2;
    return (
        <div className="console-stage" data-controls={controls}
            onPointerMove={reveal} onPointerDown={reveal}
            style={{ "--travel": progress, "--drift": Math.sin(phase) } as CSSProperties}>
            <ProtocolConsole time={progress * PROTOCOL_CONSOLE_DURATION} />
            <nav className="console-tools" aria-label="Керування панеллю">
                <button onClick={onBack}>Назад</button>
                <button onClick={() => setPlaying(!playing)}>
                    {playing ? "Пауза" : "Відтворити"}
                </button>
            </nav>
        </div>
    );
}
