import type { ReactNode } from "react";
import type { SlideDefinition, TableExample } from "../data/types";
import {
    firstServer,
    firstServerLines,
    firstServerLineWord,
} from "../data/first-server";
import { CodeCard } from "./CodeCard";
import type { CopyCode } from "./CodeCard";
import { SlideSources } from "./SlideSources";

function DataTable({
    caption,
    columns,
    rows,
    className,
}: TableExample & { className: string }) {
    return (
        <table className={className}>
            <caption>{caption}</caption>
            <thead>
                <tr>
                    {columns.map((column) => (
                        <th key={column} scope="col">
                            {column}
                        </th>
                    ))}
                </tr>
            </thead>
            <tbody>
                {rows.map((row, index) => (
                    <tr key={index}>
                        {row.map((cell, col) => (
                            <td key={col}>{cell}</td>
                        ))}
                    </tr>
                ))}
            </tbody>
        </table>
    );
}

function Disclosure({
    label,
    children,
}: {
    label: string;
    children: ReactNode;
}) {
    return (
        <details className="wire">
            <summary>{label}</summary>
            {children}
        </details>
    );
}

function SlideCode({
    slide,
    onCopy,
}: {
    slide: SlideDefinition;
    onCopy?: CopyCode;
}) {
    if (!slide.code) return null;

    const code = (
        <CodeCard
            code={slide.code}
            file={slide.codeTitle || slide.file}
            language={slide.language}
            caption={slide.codeCaption}
            onCopy={onCopy}
        />
    );
    if (slide.foldCode || slide.example)
        return <Disclosure label="Приклад на C#">{code}</Disclosure>;
    if (slide.interactive === "host")
        return (
            <Disclosure label="Код демо · перевірка Host">{code}</Disclosure>
        );

    return code;
}

type WorkbenchProps = {
    slide: SlideDefinition;
    visual?: ReactNode;
    exercise?: ReactNode;
    onCopy?: CopyCode;
};

export function SlideWorkbench({
    slide,
    visual,
    exercise,
    onCopy,
}: WorkbenchProps) {
    const {
        beats,
        example,
        keyRows,
        explain,
        client,
        wire,
        command,
        full,
        note,
    } = slide;
    return (
        <div className="workbench">
            {visual}
            {beats && (
                <div className="story-beats">
                    {beats.map(({ title, text }) => (
                        <div className="story-beat" key={title}>
                            <h2>{title}</h2>
                            <p>{text}</p>
                        </div>
                    ))}
                </div>
            )}
            {example && <DataTable className="header-example" {...example} />}
            <SlideCode slide={slide} onCopy={onCopy} />
            {keyRows && (
                <DataTable
                    className="key-table"
                    caption="Позиції у block · нумерація з нуля"
                    columns={["Клієнт / сервер", "Довжини", "Призначення"]}
                    rows={keyRows}
                />
            )}
            {explain && (
                <dl className="code-explanation">
                    {explain.map(([term, meaning]) => (
                        <div key={term}>
                            <dt>{term}</dt>
                            <dd>{meaning}</dd>
                        </div>
                    ))}
                </dl>
            )}
            <SlideSources slide={slide} />
            {client && (
                <Disclosure label="Клієнт у браузері · JavaScript">
                    <CodeCard
                        code={client}
                        file="Браузер"
                        language="JavaScript"
                        caption="ПРИКЛАД · підключення до відповідного endpoint"
                        onCopy={onCopy}
                    />
                </Disclosure>
            )}
            {exercise}
            {wire && (
                <Disclosure label="На дроті / схема">
                    <pre>{wire}</pre>
                </Disclosure>
            )}
            {command && (
                <Disclosure label="Команда для показу">
                    <pre>
                        <code>{command}</code>
                    </pre>
                </Disclosure>
            )}
            {full && (
                <Disclosure
                    label={`Повний HTTP/0.9-сервер · ${firstServerLines} ${firstServerLineWord}`}
                >
                    <CodeCard
                        code={firstServer}
                        file="Program.cs"
                        caption="ПОВНИЙ ФАЙЛ · 01_1991_HTTP0.9_GET"
                        onCopy={onCopy}
                    />
                </Disclosure>
            )}
            {note && <p className="note">{note}</p>}
        </div>
    );
}
