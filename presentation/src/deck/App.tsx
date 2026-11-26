import { useCallback, useEffect, useRef, useState } from "react";
import { slides } from "../data/slides";
import { RunFinale } from "../demos/RunFinale";
import { Slide } from "../slides/Slide";
import { ContentsDialog } from "./ContentsDialog";
import { TopBar, SlideNavigation } from "./DeckControls";
import { useDeckNavigation } from "./useDeckNavigation";
import { useSwipe } from "./useSwipe";

const slideCount = slides.length;

export function App() {
    const { index, goTo, heading } = useDeckNavigation();
    const dialog = useRef<HTMLDialogElement>(null);
    const [toast, setToast] = useState("");
    const toastTimer = useRef<number>(undefined);
    const swipe = useSwipe((direction) => goTo(index + direction));

    const copyCode = useCallback(async (code: string) => {
        let message = "Код скопійовано";
        try {
            await navigator.clipboard.writeText(code);
        } catch {
            message = "Виділи код і скопіюй вручну";
        }

        window.clearTimeout(toastTimer.current);
        setToast(message);
        toastTimer.current = window.setTimeout(() => setToast(""), 2000);
    }, []);

    useEffect(() => () => window.clearTimeout(toastTimer.current), []);

    useEffect(() => {
        function onKeyDown(event: KeyboardEvent) {
            if (
                event.defaultPrevented ||
                dialog.current?.open ||
                event.altKey ||
                event.ctrlKey ||
                event.metaKey ||
                (event.target instanceof Element &&
                    event.target.closest(
                        "pre, button, a, input, summary, select, textarea, video, [contenteditable]",
                    ))
            )
                return;

            const destinations: Record<string, number> = {
                ArrowRight: index + 1,
                PageDown: index + 1,
                ArrowLeft: index - 1,
                PageUp: index - 1,
                Home: 0,
                End: slideCount - 1,
            };
            if (!Object.hasOwn(destinations, event.key)) return;

            event.preventDefault();
            goTo(destinations[event.key]);
        }

        document.addEventListener("keydown", onKeyDown);
        return () => document.removeEventListener("keydown", onKeyDown);
    }, [goTo, index]);

    function openContents(button: HTMLButtonElement) {
        if (!dialog.current || dialog.current.open) return;

        button.focus({ preventScroll: true });
        dialog.current.showModal();
        dialog.current
            .querySelector('[aria-current="step"]')
            ?.scrollIntoView({ block: "center" });
    }

    const [printing, setPrinting] = useState(false);
    useEffect(() => {
        const startPrint = () => setPrinting(true);
        const endPrint = () => setPrinting(false);
        window.addEventListener("beforeprint", startPrint);
        window.addEventListener("afterprint", endPrint);
        return () => {
            window.removeEventListener("beforeprint", startPrint);
            window.removeEventListener("afterprint", endPrint);
        };
    }, []);

    if (slides[index].id === "run" && !printing) {
        return (
            <main aria-label={slides[index].title} {...swipe}>
                <RunFinale onBack={() => goTo(index - 1)} />
            </main>
        );
    }

    return (
        <>
            <TopBar
                progress={((index + 1) / slideCount) * 100}
                firstSlideId={slides[0].id}
                onOpenContents={openContents}
            />
            <main
                className="deck"
                aria-label="Презентація еволюції HTTP"
                {...swipe}
            >
                {slides.map((slide, position) => (
                    <Slide
                        key={slide.id}
                        slide={slide}
                        position={position + 1}
                        total={slideCount}
                        active={position === index}
                        headingRef={position === index ? heading : undefined}
                        onCopy={copyCode}
                    />
                ))}
            </main>
            <SlideNavigation
                index={index}
                total={slideCount}
                chapter={slides[index].chapter}
                onNavigate={goTo}
                onOpenContents={openContents}
            />
            <ContentsDialog
                slides={slides}
                index={index}
                dialog={dialog}
                onNavigate={goTo}
            />
            <div
                className={toast ? "toast show" : "toast"}
                role="status"
                aria-live="polite"
            >
                {toast}
            </div>
        </>
    );
}
