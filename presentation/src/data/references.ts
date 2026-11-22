export const references = {
    "linux-pss": [
        "Linux · PSS",
        "https://www.kernel.org/doc/html/latest/filesystems/proc.html",
    ],
    "kestrel-http3": [
        "Kestrel · HTTP/3",
        "https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-10.0",
    ],
    "nginx-http3": [
        "nginx · HTTP/3",
        "https://nginx.org/en/docs/http/ngx_http_v3_module.html",
    ],
    w3c: ["W3C · HTTP/0.9", "https://www.w3.org/Protocols/HTTP/AsImplemented"],
    "1945": ["RFC 1945", "https://www.rfc-editor.org/rfc/rfc1945.html"],
    "2068": ["RFC 2068", "https://www.rfc-editor.org/rfc/rfc2068.html"],
    "6101": ["RFC 6101", "https://www.rfc-editor.org/rfc/rfc6101.html"],
    "2246": ["RFC 2246", "https://www.rfc-editor.org/rfc/rfc2246.html"],
    "3268": ["RFC 3268", "https://www.rfc-editor.org/rfc/rfc3268.html"],
    "7540": ["RFC 7540", "https://www.rfc-editor.org/rfc/rfc7540.html"],
    "7541": ["RFC 7541", "https://www.rfc-editor.org/rfc/rfc7541.html"],
    "7541-size": [
        "RFC 7541 §4.1",
        "https://www.rfc-editor.org/rfc/rfc7541.html#section-4.1",
    ],
    "9113": ["RFC 9113", "https://www.rfc-editor.org/rfc/rfc9113.html"],
    "8446": ["RFC 8446", "https://www.rfc-editor.org/rfc/rfc8446.html"],
    "9000": ["RFC 9000", "https://www.rfc-editor.org/rfc/rfc9000.html"],
    "9001": ["RFC 9001", "https://www.rfc-editor.org/rfc/rfc9001.html"],
    "9204": ["RFC 9204", "https://www.rfc-editor.org/rfc/rfc9204.html"],
    "9114": ["RFC 9114", "https://www.rfc-editor.org/rfc/rfc9114.html"],
    "sse-draft": [
        "W3C · SSE 2009",
        "https://www.w3.org/TR/2009/WD-eventsource-20090423/",
    ],
    "sse-current": [
        "HTML · EventSource",
        "https://html.spec.whatwg.org/multipage/server-sent-events.html",
    ],
    "6455": [
        "RFC 6455 · WebSocket",
        "https://www.rfc-editor.org/rfc/rfc6455.html",
    ],
    "websocket-draft": [
        "W3C · API 2009",
        "https://www.w3.org/TR/2009/WD-websockets-20090423/",
    ],
    cern: [
        "CERN · історія вебу",
        "https://home.cern/science/computing/the-birth-of-the-web/short-history-web/",
    ],
    "netscape-1994": [
        "Netscape · анонс 1994 (архів)",
        "https://www.krsaborio.net/internet/research/1994/1215.htm",
    ],
    "ssl-history": [
        "Тім Діркс · історія SSL",
        "https://tim.dierks.org/2014/05/security-standards-and-name-changes-in.html",
    ],
    "6176": [
        "RFC 6176 · недоліки SSL2",
        "https://www.rfc-editor.org/rfc/rfc6176.html",
    ],
} satisfies Record<string, readonly [label: string, url: string]>;

export type ReferenceId = keyof typeof references;
