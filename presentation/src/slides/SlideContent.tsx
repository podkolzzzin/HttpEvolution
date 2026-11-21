import { Fragment } from "react";
import type { ReactNode, Ref } from "react";
import type { SlideDefinition } from "../data/types";
import type { CopyCode } from "./CodeCard";
import { FinalLinks } from "./SlideSources";
import { firstServerLines, firstServerLineWord } from "../data/first-server";
import { SlideWorkbench } from "./SlideWorkbench";
import {
    MemoryBenchmark,
    MemoryMethod,
    MemoryTimeline,
} from "./MemoryComparison";

function HeroRoute() {
    return (
        <>
            <div
                className="route"
                aria-label="HTTP 0.9, HTTP 1.x, HTTP 2, HTTP 3"
            >
                <span>0.9</span>
                <i>→</i>
                <span>1.x</span>
                <i>→</i>
                <span>2</span>
                <i>→</i>
                <span>3</span>
            </div>
            <div className="hero-stats">
                <span>
                    <strong>{firstServerLines}</strong>
                    {firstServerLineWord} першого сервера
                </span>
                <span>
                    <strong>3</strong>мережеві класи .NET
                </span>
            </div>
        </>
    );
}

type Props = {
    slide: SlideDefinition;
    position: number;
    total: number;
    headingRef?: Ref<HTMLHeadingElement>;
    active?: boolean;
    lead?: ReactNode;
    change?: ReactNode;
    visual?: ReactNode;
    exercise?: ReactNode;
    onCopy?: CopyCode;
};

export function SlideContent({
    slide,
    position,
    total,
    headingRef,
    active,
    lead,
    change,
    visual,
    exercise,
    onCopy,
}: Props) {
    const category = /^(SSL|TLS|HTTPS)/.test(slide.chapter)
        ? "tls"
        : /^(QUIC|HTTP\/3)/.test(slide.chapter)
          ? "quic"
          : "";
    const classes = [
        "slide",
        category,
        (slide.interactive === "ssl-traffic" || slide.interactive === "host") &&
            "traffic-slide",
        slide.walkthrough && "ssl-walkthrough",
        !slide.code && "narrative",
        slide.hero && "hero",
        active && "current",
    ]
        .filter(Boolean)
        .join(" ");
    const changeBlock = (change ?? slide.change) && (
        <div className="change">
            {change ?? (
                <>
                    <span className="change-label">
                        {slide.changeLabel || "Що змінюємо"}
                    </span>
                    {slide.change}
                </>
            )}
        </div>
    );

    return (
        <section
            className={classes}
            id={slide.id}
            aria-label={`Слайд ${position} з ${total}`}
            hidden={active === false}
        >
            {!slide.walkthrough && (
                <div className="eyebrow">
                    <span className="year">{slide.year}</span>
                    <span className="dot" />
                    <span>{slide.chapter}</span>
                </div>
            )}
            <div className="layout">
                <div className="story">
                    <h1 tabIndex={-1} ref={headingRef}>
                        {slide.title.split("\n").map((line, index) => (
                            <Fragment key={index}>
                                {index > 0 && <br />}
                                {line}
                            </Fragment>
                        ))}
                    </h1>
                    <p className="lead">{lead ?? slide.why}</p>
                    {slide.memoryTimeline ? <MemoryBenchmark /> : changeBlock}
                    {slide.hero && <HeroRoute />}
                    {slide.final && <FinalLinks />}
                    {slide.memory && (
                        <div className="connection-memory">
                            <h2>Що тримаємо в пам’яті</h2>
                            <p>{slide.memory}</p>
                        </div>
                    )}
                </div>
                <SlideWorkbench
                    slide={slide}
                    visual={visual}
                    memoryTimeline={
                        slide.memoryTimeline && (
                            <>
                                <MemoryTimeline />
                                {changeBlock}
                                <MemoryMethod />
                            </>
                        )
                    }
                    exercise={exercise}
                    onCopy={onCopy}
                />
            </div>
            <div className="slide-base">
                <span className="section-color" />
                <span>HTTP EVOLUTION</span>
                <span className="rule" />
                <span>
                    {String(position).padStart(2, "0")} / {total}
                </span>
            </div>
        </section>
    );
}
