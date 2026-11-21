import { useId } from "react";
import type { MouseEvent, RefObject } from "react";
import { Fragment } from "react";
import type { SlideDefinition } from "../data/types";

type Props = {
    slides: readonly SlideDefinition[];
    index: number;
    dialog: RefObject<HTMLDialogElement | null>;
    onNavigate: (index: number) => void;
};

export function ContentsDialog({ slides, index, dialog, onNavigate }: Props) {
    const titleId = useId();

    function onBackdropClick(event: MouseEvent<HTMLDialogElement>) {
        if (event.target !== event.currentTarget) return;

        const { left, top, right, bottom } =
            event.currentTarget.getBoundingClientRect();
        if (
            event.clientX < left ||
            event.clientX > right ||
            event.clientY < top ||
            event.clientY > bottom
        ) {
            event.currentTarget.close();
        }
    }

    return (
        <dialog
            className="contents"
            ref={dialog}
            aria-labelledby={titleId}
            onClick={onBackdropClick}
        >
            <div className="dialog-top">
                <h2 id={titleId}>Увесь маршрут</h2>
                <button
                    className="close-menu"
                    type="button"
                    aria-label="Закрити зміст"
                    onClick={() => dialog.current?.close()}
                >
                    ×
                </button>
            </div>
            <nav className="topics" aria-label="Зміст презентації">
                {slides.map((slide, position) => (
                    <Fragment key={slide.id}>
                        {slide.chapter !== slides[position - 1]?.chapter && (
                            <div className="topics-group">{slide.chapter}</div>
                        )}
                        <button
                            type="button"
                            className="topic"
                            aria-current={
                                position === index ? "step" : undefined
                            }
                            onClick={() => {
                                dialog.current?.close();
                                onNavigate(position);
                            }}
                        >
                            <span>{String(position + 1).padStart(2, "0")}</span>
                            <span>{slide.year}</span>
                            <strong>{slide.title.replaceAll("\n", " ")}</strong>
                        </button>
                    </Fragment>
                ))}
            </nav>
        </dialog>
    );
}
