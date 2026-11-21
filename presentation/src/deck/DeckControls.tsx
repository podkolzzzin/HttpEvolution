type TopBarProps = {
    progress: number;
    firstSlideId: string;
    onOpenContents: (button: HTMLButtonElement) => void;
};

export function TopBar({
    progress,
    firstSlideId,
    onOpenContents,
}: TopBarProps) {
    return (
        <header className="topbar">
            <a
                className="brand"
                href={`#${firstSlideId}`}
                aria-label="На перший слайд"
            >
                <span className="brand-mark" aria-hidden="true">
                    H/
                </span>{" "}
                HTTP Evolution
            </a>
            <span className="top-sub">Від байтів до потоків</span>
            <button
                className="menu-button"
                type="button"
                aria-haspopup="dialog"
                onClick={(event) => onOpenContents(event.currentTarget)}
            >
                <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.6"
                    aria-hidden="true"
                >
                    <path d="M4 6h16M4 12h16M4 18h16" />
                </svg>
                Зміст
            </button>
            <div
                className="top-progress"
                style={{ width: `${progress}%` }}
                aria-hidden="true"
            />
        </header>
    );
}

type SlideNavigationProps = {
    index: number;
    total: number;
    chapter: string;
    onNavigate: (index: number) => void;
    onOpenContents: (button: HTMLButtonElement) => void;
};

export function SlideNavigation({
    index,
    total,
    chapter,
    onNavigate,
    onOpenContents,
}: SlideNavigationProps) {
    return (
        <footer className="bottom" aria-label="Навігація слайдами">
            <button
                className="nav-btn prev"
                type="button"
                aria-label="Попередній слайд"
                disabled={index === 0}
                onClick={() => onNavigate(index - 1)}
            >
                <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.8"
                    aria-hidden="true"
                >
                    <path d="m14 6-6 6 6 6" />
                </svg>
                <span>Назад</span>
            </button>
            <button
                className="counter"
                type="button"
                aria-label="Відкрити зміст"
                aria-haspopup="dialog"
                onClick={(event) => onOpenContents(event.currentTarget)}
            >
                <strong>{String(index + 1).padStart(2, "0")}</strong> / {total}
                <small>{chapter}</small>
            </button>
            <span className="swipe-hint">← свайп / клавіші →</span>
            <button
                className="nav-btn next"
                type="button"
                disabled={index === total - 1}
                onClick={() => onNavigate(index + 1)}
            >
                <span>{index === total - 1 ? "Кінець" : "Далі"}</span>
                <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.8"
                    aria-hidden="true"
                >
                    <path d="m10 6 6 6-6 6" />
                </svg>
            </button>
        </footer>
    );
}
