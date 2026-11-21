import type { ReactNode } from "react";

export type CopyCode = (code: string) => void;

const tokenPattern =
    /(\/\/[^\n]*|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\b(?:using|var|if|else|new|return|for|foreach|while|continue|break|true|false|null|int|byte|bool|throw|await|static|sealed|class|public|void|in|is|not|or|stackalloc)\b|\b(?:0x[\da-fA-F]+|\d+)\b)/g;

function tokenClass(token: string) {
    if (token.startsWith("//")) return "comment";
    if (/^["']/.test(token)) return "str";
    if (/^\d/.test(token)) return "num";
    return "kw";
}

function highlight(code: string) {
    const parts: ReactNode[] = [];
    let offset = 0;

    for (const { 0: token, index } of code.matchAll(tokenPattern)) {
        parts.push(code.slice(offset, index));
        parts.push(
            <span className={tokenClass(token)} key={index}>
                {token}
            </span>,
        );
        offset = index + token.length;
    }
    parts.push(code.slice(offset));

    return parts;
}

type Props = {
    code: string;
    file?: string;
    language?: string;
    caption?: string;
    onCopy?: CopyCode;
};

export function CodeCard({
    code,
    file,
    language = "C#",
    caption = "ФРАГМЕНТ · деталі й перевірки скорочено",
    onCopy,
}: Props) {
    return (
        <div className="code-card">
            <div className="code-top">
                <span>
                    {file} <b aria-hidden="true">·</b> {language}
                </span>
                {onCopy && (
                    <button
                        type="button"
                        className="copy"
                        aria-label="Копіювати код"
                        onClick={() => onCopy(code)}
                    >
                        <svg
                            viewBox="0 0 20 20"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="1.4"
                            aria-hidden="true"
                        >
                            <rect x="6" y="6" width="10" height="11" rx="2" />
                            <path d="M4 13H3a1 1 0 0 1-1-1V3a1 1 0 0 1 1-1h8a1 1 0 0 1 1 1v1" />
                        </svg>
                        <span>Копіювати</span>
                    </button>
                )}
            </div>
            <pre tabIndex={0} aria-label="Приклад коду">
                <code>{highlight(code)}</code>
            </pre>
            <div className="code-foot">{caption}</div>
        </div>
    );
}
