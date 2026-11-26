import { useRef } from "react";
import type { PointerEvent } from "react";

export function useSwipe(onSwipe: (direction: -1 | 1) => void) {
    const start = useRef<{
        id: number;
        x: number;
        y: number;
        time: number;
    } | null>(null);

    return {
        onPointerDown(event: PointerEvent<HTMLElement>) {
            if (
                event.pointerType !== "touch" ||
                !event.isPrimary ||
                (event.target instanceof Element &&
                    event.target.closest(
                        "pre, button, a, summary, input, select, details, video",
                    ))
            )
                return;

            start.current = {
                id: event.pointerId,
                x: event.clientX,
                y: event.clientY,
                time: event.timeStamp,
            };
        },
        onPointerUp(event: PointerEvent<HTMLElement>) {
            const origin = start.current;
            if (!origin || origin.id !== event.pointerId) return;

            start.current = null;
            const dx = event.clientX - origin.x;
            const dy = event.clientY - origin.y;
            if (
                Math.abs(dx) > 65 &&
                Math.abs(dy) < 40 &&
                event.timeStamp - origin.time < 650 &&
                !window.getSelection()?.toString()
            ) {
                onSwipe(dx < 0 ? 1 : -1);
            }
        },
        onPointerCancel(event: PointerEvent<HTMLElement>) {
            if (start.current?.id === event.pointerId) start.current = null;
        },
    };
}
