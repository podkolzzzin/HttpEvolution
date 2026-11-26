import { memo } from "react";
import type { ReactNode } from "react";

const csharpKeywords = new Set(
    `abstract and as async await base bool break byte case catch char checked class const
    continue decimal default delegate do double else enum event explicit extern false
    finally fixed float for foreach goto if implicit in int interface internal is lock
    long namespace new not null object operator or out override params private
    protected public readonly ref return sbyte sealed short sizeof stackalloc static
    string struct switch this throw true try typeof uint ulong unchecked unsafe
    ushort using var virtual void volatile while`.split(/\s+/),
);

const javascriptKeywords = new Set(
    `async await break case catch class const continue debugger default delete do else
    export extends false finally for function if import in instanceof let new null
    return static super switch this throw true try typeof undefined var void while
    with yield`.split(/\s+/),
);

const comments = /(?<comment>\/\/[^\r\n]*|\/\*[\s\S]*?\*\/)/;
const identifier = /@?[\p{ID_Start}_$][\p{ID_Continue}$]*/u;

function alternatives(...patterns: RegExp[]) {
    return patterns.map(({ source }) => source).join("|");
}

const csharpStrings = alternatives(
    /\$*(?<fence>"{3,})(?!")[\s\S]*?\k<fence>(?!")(?:[uU]8\b)?/,
    /(?:\$@|@\$|@)"(?:""|[^"])*"(?:[uU]8\b)?/,
    /\$?"(?:\\.|[^"\\\r\n])*"(?:[uU]8\b)?/,
    /'(?:\\.|[^'\\\r\n])*'/,
);
const csharpNumbers = alternatives(
    /(?:0[xX][_\da-fA-F]+|0[bB][_01]+)(?:[uU][lL]?|[lL][uU]?)?/,
    /(?:\d[\d_]*(?:\.\d[\d_]*)?|\.\d[\d_]*)(?:[eE][+-]?\d[\d_]*)?(?:[fFdDmM]|[uU][lL]?|[lL][uU]?)?/,
);

const javascriptStrings = alternatives(
    /"(?:\\.|[^"\\\r\n])*"/,
    /'(?:\\.|[^'\\\r\n])*'/,
    /`(?:\\[\s\S]|[^`\\])*`/,
);
const javascriptNumbers = alternatives(
    /(?:0[xX][\da-fA-F_]+|0[bB][01_]+|0[oO][0-7_]+)n?/,
    /(?:\d[\d_]*(?:\.[\d_]*)?|\.\d[\d_]*)(?:[eE][+-]?\d[\d_]*)?n?/,
);

function tokenPattern(strings: string, numbers: string) {
    return new RegExp(
        `${comments.source}|(?<string>${strings})|(?<number>${numbers})|${identifier.source}`,
        "gu",
    );
}

const syntax = {
    csharp: {
        pattern: tokenPattern(csharpStrings, csharpNumbers),
        keywords: csharpKeywords,
    },
    javascript: {
        pattern: tokenPattern(javascriptStrings, javascriptNumbers),
        keywords: javascriptKeywords,
    },
};

export const HighlightedCode = memo(function HighlightedCode({
    code,
    language,
}: {
    code: string;
    language?: string;
}) {
    const { pattern, keywords } =
        language === "JavaScript" ? syntax.javascript : syntax.csharp;
    const parts: ReactNode[] = [];
    let offset = 0;

    for (const { 0: token, index, groups } of code.matchAll(pattern)) {
        const kind = groups?.comment
            ? "comment"
            : groups?.string
              ? "string"
              : groups?.number
                ? "number"
                : keywords.has(token)
                  ? "keyword"
                  : null;
        if (!kind) continue;

        parts.push(
            code.slice(offset, index),
            <span className={`syntax-${kind}`} key={index}>
                {token}
            </span>,
        );
        offset = index + token.length;
    }
    parts.push(code.slice(offset));

    return <code className="highlighted-code">{parts}</code>;
});
