import { useLayoutEffect, useRef } from "react";

export function RunFinale({ onBack }: { onBack: () => void }) {
    const host = useRef<HTMLDivElement>(null);
    const back = useRef(onBack);
    useLayoutEffect(() => { back.current = onBack; }, [onBack]);
    useLayoutEffect(() => {
        let active = true;
        let dispose: (() => void) | undefined;
        void import("./finale/player.mjs").then(({ mountFinale }) => {
            if (active && host.current) dispose = mountFinale(host.current, () => back.current());
        });
        return () => {
            active = false;
            dispose?.();
        };
    }, []);
    return <div style={{position: "fixed", inset: 0, background: "#0a1316"}} ref={host} />;
}
