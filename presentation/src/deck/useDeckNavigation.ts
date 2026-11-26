import {
    useCallback,
    useEffect,
    useLayoutEffect,
    useRef,
    useState,
} from "react";
import { slides } from "../data/slides";

const storageKey = "http-evolution-slide";
const lastIndex = slides.length - 1;

const legacySlides: Record<string, string> = {
    "connection-memory": "server-memory",
    "head-of-line": "mobile-loss",
    "quic-udp": "quic-revolution",
    "http3-response": "http3-arrives",
    ...Object.fromEntries(["ssl3-premaster", "ssl3-master", "ssl3-keys", "ssl3-finished",
        "ssl3-mac", "ssl3-records", "ssl3-pin", "https-price"].map(id => [id, "ssl3-messages"])),
};

function indexFromId(id: string | null) {
    const slideId = id && (legacySlides[id] ?? id);
    return Math.max(0, slides.findIndex((slide) => slide.id === slideId));
}

function indexFromHash() {
    try {
        const id = decodeURIComponent(window.location.hash.slice(1));
        return indexFromId(id);
    } catch {
        return 0;
    }
}

function initialIndex() {
    if (window.location.hash) return indexFromHash();

    try {
        const savedId = localStorage.getItem(storageKey);
        return indexFromId(savedId);
    } catch {
        return 0;
    }
}

export function useDeckNavigation() {
    const [index, setIndex] = useState(initialIndex);
    const previousIndex = useRef(index);
    const heading = useRef<HTMLHeadingElement>(null);
    const slide = slides[index];

    const goTo = useCallback(
        (requestedIndex: number) => {
            const nextIndex = Math.max(0, Math.min(lastIndex, requestedIndex));
            if (nextIndex === index) return;

            window.history.pushState(null, "", "#" + slides[nextIndex].id);
            setIndex(nextIndex);
        },
        [index],
    );

    useEffect(() => {
        const syncLocation = () => setIndex(indexFromHash());
        const previousScrollRestoration = window.history.scrollRestoration;
        window.history.scrollRestoration = "manual";
        window.addEventListener("hashchange", syncLocation);
        window.addEventListener("popstate", syncLocation);

        return () => {
            window.history.scrollRestoration = previousScrollRestoration;
            window.removeEventListener("hashchange", syncLocation);
            window.removeEventListener("popstate", syncLocation);
        };
    }, []);

    useLayoutEffect(() => {
        if (!window.location.hash) {
            window.history.replaceState(null, "", "#" + slide.id);
        }

        document.title =
            slide.title.replaceAll("\n", " ") + " — HTTP Evolution";
        try {
            localStorage.setItem(storageKey, slide.id);
        } catch {
            // Hash navigation also works when storage is unavailable.
        }

        window.scrollTo(0, 0);
        if (previousIndex.current !== index) {
            heading.current?.focus({ preventScroll: true });
        }
        previousIndex.current = index;
    }, [index, slide]);

    return { index, goTo, heading };
}
