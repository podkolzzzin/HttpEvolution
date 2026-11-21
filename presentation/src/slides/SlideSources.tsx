import type { ReactNode } from "react";
import { references } from "../data/references";
import type { SlideDefinition } from "../data/types";

const repository = "https://github.com/podkolzzzin/HttpEvolution";
const branch = "chronological";

export function ExternalLink({
    href,
    children,
}: {
    href: string;
    children: ReactNode;
}) {
    return (
        <a href={href} target="_blank" rel="noopener">
            {children}
        </a>
    );
}

export function SlideSources({ slide }: { slide: SlideDefinition }) {
    const { file, sourceFile, step, rfc, extraRefs = [] } = slide;
    const referenceIds = rfc ? [rfc, ...extraRefs] : extraRefs;

    return (
        <div className="sources">
            {file && (
                <ExternalLink
                    href={`${repository}/blob/${branch}/${sourceFile || file}`}
                >
                    Код
                </ExternalLink>
            )}
            {step && (
                <ExternalLink href={`${repository}/tree/${branch}/${step}`}>
                    Етап {step.split("_")[0]}
                </ExternalLink>
            )}
            {referenceIds.map((id) => (
                <ExternalLink href={references[id][1]} key={id}>
                    {references[id][0]}
                </ExternalLink>
            ))}
        </div>
    );
}

export function FinalLinks() {
    return (
        <div className="final-links">
            <ExternalLink href={`${repository}/tree/${branch}`}>
                Відкрити репозиторій ↗
            </ExternalLink>
            <ExternalLink href={`${repository}/pull/1`}>
                PR для перегляду ↗
            </ExternalLink>
        </div>
    );
}
