import type { ReactNode } from "react";

type Props = { time: number };
const ink = "#20352b",
    leaf = "#a7cf58",
    paper = "#f8f7ef",
    soft = "#fffef9",
    muted = "#708077";
function Shell({ title, children }: { title: string; children: ReactNode }) {
    return (
        <div
            style={{
                height: "100%",
                minHeight: 360,
                overflow: "hidden",
                background: paper,
                color: ink,
                fontFamily: "Inter,system-ui,sans-serif",
                display: "grid",
                gridTemplateRows: "auto 1fr",
            }}
        >
            <header
                style={{
                    padding: "14px 4% 10px",
                    borderBottom: `2px solid ${ink}`,
                    display: "flex",
                    justifyContent: "space-between",
                    fontSize: "clamp(11px,1.2vw,18px)",
                    fontWeight: 700,
                    letterSpacing: ".1em",
                }}
            >
                <span>HTTP · ІСТОРИЧНИЙ ПРОТОКОЛ</span>
                <span style={{ color: muted }}>1996—2021</span>
            </header>
            <main
                style={{
                    display: "grid",
                    gridTemplateRows: "auto 1fr",
                    minHeight: 0,
                }}
            >
                <h2
                    style={{
                        margin: "2% 4% 0",
                        fontSize: "clamp(22px,3vw,46px)",
                        letterSpacing: "-.03em",
                    }}
                >
                    {title}
                </h2>
                <div style={{ minHeight: 0, padding: "1% 3% 3%" }}>
                    <svg
                        viewBox="0 0 1000 470"
                        preserveAspectRatio="xMidYMid meet"
                        style={{ width: "100%", height: "100%" }}
                    >
                        {children}
                    </svg>
                </div>
            </main>
        </div>
    );
}
function Card({
    x,
    y,
    w,
    h,
    title,
    detail,
    active = false,
}: {
    x: number;
    y: number;
    w: number;
    h: number;
    title: string;
    detail?: string;
    active?: boolean;
}) {
    return (
        <g>
            <rect
                x={x}
                y={y}
                width={w}
                height={h}
                rx="14"
                fill={active ? "#eaf2d8" : soft}
                stroke={ink}
                strokeWidth="2.5"
            />
            <text
                x={x + 18}
                y={y + 34}
                fill={ink}
                fontSize="19"
                fontWeight="700"
            >
                {title}
            </text>
            {detail && (
                <text x={x + 18} y={y + 63} fill={muted} fontSize="15">
                    {detail}
                </text>
            )}
        </g>
    );
}
function Line({
    x1,
    y1,
    x2,
    y2,
    label,
}: {
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    label?: string;
}) {
    const head = x2 >= x1 ? 10 : -10;
    return (
        <g>
            <path
                d={`M${x1} ${y1}L${x2} ${y2}m${-head} -7 ${head} 7 ${-head} 7`}
                stroke={ink}
                strokeWidth="2.5"
                fill="none"
            />
            {label && (
                <text
                    x={(x1 + x2) / 2}
                    y={y1 - 10}
                    textAnchor="middle"
                    fontSize="14"
                    fill={ink}
                >
                    {label}
                </text>
            )}
        </g>
    );
}
export function SslIdentity({ time }: Props) {
    const t = Math.max(0, time),
        b = t < 6 ? 0 : t < 12 ? 1 : t < 18 ? 2 : t < 25 ? 3 : t < 30 ? 4 : 5,
        start = [0, 6, 12, 18, 25, 30][b],
        p = Math.min(1, Math.max(0, (t - start) / [6, 6, 6, 7, 5, 6][b]));
    const titles = [
        "Сертифікат сервера",
        "Перевірка сервера",
        "Pin у демо",
        "Передача секрету",
        "Виведення ключів",
        "Захищені записи",
    ];
    return (
        <Shell title={titles[b]}>
            {b === 0 && (
                <>
                    <Card
                        x={65}
                        y={55}
                        w={355}
                        h={350}
                        title="СЕРТИФІКАТ X.509"
                        detail="Поля, засвідчені підписом CA"
                        active
                    />
                    {[
                        "Ім’я / SAN · api.example.test",
                        "Строк · NotBefore — NotAfter",
                        "Емітент · Example Issuing CA",
                        "Відкритий RSA-ключ",
                        "Підпис CA",
                    ].map((s, i) => (
                        <g key={s}>
                            <rect
                                x="88"
                                y={145 + i * 48}
                                width="310"
                                height="38"
                                rx="5"
                                fill={
                                    i === Math.min(4, Math.floor(p * 5))
                                        ? leaf
                                        : "#edf0e7"
                                }
                            />
                            <text
                                x="100"
                                y={170 + i * 48}
                                fontSize="14"
                                fill={ink}
                            >
                                {s}
                            </text>
                        </g>
                    ))}
                    <Card
                        x={580}
                        y={130}
                        w={350}
                        h={190}
                        title="Сервер надсилає"
                        detail="сертифікат із відкритим ключем"
                    />
                    <Line x1={430} y1={225} x2={560} y2={225} label="підпис" />
                </>
            )}
            {b === 1 &&
                [
                    "Ім’я сайту збігається із SAN",
                    "Сертифікат чинний зараз",
                    "Підписи ведуть до довіреного кореня",
                ].map((s, i) => (
                    <g key={s}>
                        <Card
                            x={90}
                            y={45 + i * 125}
                            w={820}
                            h={92}
                            title={`${i + 1}  ${s}`}
                            active={i < Math.floor(p * 3)}
                        />
                        <circle
                            cx="865"
                            cy={91 + i * 125}
                            r="16"
                            fill={i < Math.floor(p * 3) ? leaf : soft}
                            stroke={ink}
                            strokeWidth="2"
                        />
                        <text
                            x="865"
                            y={98 + i * 125}
                            textAnchor="middle"
                            fontSize="19"
                        >
                            {i < Math.floor(p * 3) ? "✓" : "·"}
                        </text>
                    </g>
                ))}
            {b === 2 && (
                <>
                    <Card
                        x={100}
                        y={55}
                        w={800}
                        h={130}
                        title="ДЕМО · САМОПІДПИСАНИЙ LOCALHOST"
                        detail="Base64(SHA-256(DER(SPKI)))"
                        active
                    />
                    <Card
                        x={110}
                        y={260}
                        w={320}
                        h={100}
                        title="Обчислений pin"
                        active={p > 0.3}
                    />
                    <text
                        x="500"
                        y="325"
                        textAnchor="middle"
                        fontSize="40"
                        fontWeight="700"
                    >
                        =
                    </text>
                    <Card
                        x={570}
                        y={260}
                        w={320}
                        h={100}
                        title="Заданий заздалегідь pin"
                        active={p > 0.65}
                    />
                    {p > 0.85 && (
                        <text
                            x="500"
                            y="420"
                            textAnchor="middle"
                            fontSize="24"
                            fontWeight="700"
                        >
                            ✓ Відбитки збігаються
                        </text>
                    )}
                </>
            )}
            {b === 3 && (
                <>
                    <Card
                        x={55}
                        y={125}
                        w={220}
                        h={130}
                        title="Клієнт"
                        detail="відкритий ключ"
                    />
                    <Card
                        x={725}
                        y={125}
                        w={220}
                        h={130}
                        title="Сервер"
                        detail="приватний ключ"
                    />
                    <Line
                        x1={285}
                        y1={185}
                        x2={700}
                        y2={185}
                        label="ClientKeyExchange · RSA"
                    />
                    {p < 0.18 && (
                        <g>
                            <rect
                                x="300"
                                y="235"
                                width="210"
                                height="76"
                                rx="12"
                                fill={leaf}
                                stroke={ink}
                                strokeWidth="2"
                            />
                            <text
                                x="405"
                                y="267"
                                textAnchor="middle"
                                fontSize="17"
                                fontWeight="700"
                            >
                                premaster · 48 байтів
                            </text>
                            <text
                                x="405"
                                y="292"
                                textAnchor="middle"
                                fontSize="14"
                            >
                                секрет клієнта
                            </text>
                        </g>
                    )}
                    {p >= 0.18 && (
                        <g
                            transform={`translate(${((p - 0.18) / 0.82) * 340},0)`}
                        >
                            <rect
                                x="300"
                                y="235"
                                width="210"
                                height="76"
                                rx="12"
                                fill={leaf}
                                stroke={ink}
                                strokeWidth="2"
                            />
                            <text
                                x="405"
                                y="267"
                                textAnchor="middle"
                                fontSize="17"
                                fontWeight="700"
                            >
                                RSA-шифротекст
                            </text>
                            <text
                                x="405"
                                y="292"
                                textAnchor="middle"
                                fontSize="14"
                            >
                                ClientKeyExchange
                            </text>
                        </g>
                    )}
                    <text
                        x="500"
                        y="390"
                        textAnchor="middle"
                        fontSize="17"
                        fill={muted}
                    >
                        Відкритий ключ шифрує · приватний ключ відновлює секрет
                    </text>
                </>
            )}
            {b === 4 && (
                <>
                    <text
                        x="500"
                        y="48"
                        textAnchor="middle"
                        fontSize="21"
                        fontWeight="700"
                    >
                        Клієнт і сервер обчислюють ключі незалежно
                    </text>
                    {["КЛІЄНТ", "СЕРВЕР"].map((s, i) => (
                        <g key={s}>
                            <text
                                x="60"
                                y={150 + i * 155}
                                fontSize="17"
                                fontWeight="700"
                            >
                                {s}
                            </text>
                            <Card
                                x={150}
                                y={95 + i * 155}
                                w={200}
                                h={85}
                                title="premaster"
                                detail="спільний секрет"
                            />
                            <text x="385" y={148 + i * 155} fontSize="26">
                                +
                            </text>
                            <Card
                                x={430}
                                y={95 + i * 155}
                                w={220}
                                h={85}
                                title="client_random"
                                detail="+ server_random"
                            />
                            <Line
                                x1={660}
                                y1={138 + i * 155}
                                x2={735}
                                y2={138 + i * 155}
                            />
                            <text
                                x="755"
                                y={145 + i * 155}
                                fontSize="16"
                                fontWeight="700"
                            >
                                {p > 0.45 ? "ключі запису" : "master secret"}
                            </text>
                        </g>
                    ))}
                </>
            )}
            {b === 5 && (
                <>
                    <Card
                        x={55}
                        y={110}
                        w={245}
                        h={230}
                        title="Клієнт"
                        detail="HTTP-запит →"
                    />
                    <Card
                        x={700}
                        y={110}
                        w={245}
                        h={230}
                        title="Сервер"
                        detail="← HTTP-відповідь"
                    />
                    <Line
                        x1={315}
                        y1={165}
                        x2={680}
                        y2={165}
                        label="3DES-CBC · ключ клієнта"
                    />
                    <Line
                        x1={680}
                        y1={280}
                        x2={315}
                        y2={280}
                        label="3DES-CBC · ключ сервера"
                    />
                    <Card
                        x={385}
                        y={200}
                        w={230}
                        h={65}
                        title="Захищений запис"
                        detail="окремі ключі напрямків"
                        active
                    />
                </>
            )}
        </Shell>
    );
}
