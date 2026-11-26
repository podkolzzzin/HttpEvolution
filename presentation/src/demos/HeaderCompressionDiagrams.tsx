import { useState } from "react";
import { Choices } from "./ProtocolControls";
import "./HpackRepeat.css";

const domain = "www.example.com";

export function HpackLiteral() {
    const [part, setPart] = useState(0);
    const fields = [
        [
            "01",
            "000001",
            "01 · літерал з індексацією",
            "000001 · ім’я №1 зі статичної таблиці → :authority",
        ],
        [
            "0",
            "0001111",
            "0 · рядок без Гаффмана",
            "0001111 · довжина значення → 15 байтів",
        ],
    ];
    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>:authority → {domain}</strong>
                <span>Перший запит · літерал</span>
            </div>
            <Choices
                labels={[
                    "Байт 1 · ім’я",
                    "Байт 2 · довжина",
                    "Байти 3–17 · значення",
                ]}
                selected={part}
                onSelect={setPart}
                name="Частина літерала"
            />
            {part < 2 ? (
                <div className="bits-board">
                    <div className="bit-line">
                        <b>{fields[part][0]}</b>
                        <strong>{fields[part][1]}</strong>
                    </div>
                    <div className="bit-legend">
                        <span>{fields[part][2]}</span>
                        <span>{fields[part][3]}</span>
                    </div>
                </div>
            ) : (
                <div className="binary-domain">
                    {Array.from(domain, (letter, i) => (
                        <div key={i}>
                            <b>{letter}</b>
                            <code>
                                {letter
                                    .charCodeAt(0)
                                    .toString(2)
                                    .padStart(8, "0")}
                            </code>
                        </div>
                    ))}
                </div>
            )}
            <div className="protocol-readout">
                <b>Після декодування</b>
                <code>#62 · :authority = {domain}</code>
            </div>
        </div>
    );
}

export function HpackRepeat() {
    return (
        <div className="hp-repeat">
            <table>
                <caption>
                    Клієнт двічі запитує <code>http://{domain}/</code> через
                    те саме з’єднання HTTP/2.
                </caption>
                <thead>
                    <tr>
                        <th scope="col">Що передає клієнт</th>
                        <th scope="col">Перший запит</th>
                        <th scope="col">Повторний запит</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <th scope="row">Метод, схема, шлях</th>
                        <td><code>82 86 84</code></td>
                        <td><code>82 86 84</code></td>
                    </tr>
                    <tr>
                        <th scope="row">Домен <code>:authority</code></th>
                        <td>
                            <code>41 0F</code> <code>{domain}</code>
                            <small>2 службові байти + 15 байтів тексту</small>
                        </td>
                        <td>
                            <code className="hp-repeat-index">BE</code>
                            <small>1 байт для запису №62</small>
                        </td>
                    </tr>
                </tbody>
                <tfoot>
                    <tr>
                        <th scope="row">Блок заголовків</th>
                        <td>20 байтів</td>
                        <td>4 байти</td>
                    </tr>
                </tfoot>
            </table>
            <p>
                Після першого запиту в динамічних таблицях клієнта й сервера є
                запис №62 — <code>:authority = {domain}</code>. Отримавши
                {" "}<code>BE</code>, сервер відновлює з цього запису назву поля і значення.
            </p>
            <details>
                <summary>Як прочитати байт BE?</summary>
                <p>
                    У двійковому записі <code>BE</code> — це <code>10111110</code>.
                    Перша <code>1</code> означає «візьми пару з таблиці».
                    Решта сім бітів <code>0111110</code> дають номер 62.
                </p>
            </details>
        </div>
    );
}

const alphabet: Record<string, string> = {
    w: "1111000",
    ".": "010111",
    e: "00101",
    x: "1111001",
    a: "00011",
    m: "101001",
    p: "101011",
    l: "101000",
    c: "00100",
    o: "00111",
};
const tokens = Array.from(domain, (ch) => alphabet[ch]);
const bitCount = tokens.join("").length;
const padding = (8 - (bitCount % 8)) % 8;
const packed = (tokens.join("") + "1".repeat(padding)).match(/.{8}/g)!;

export function HuffmanBits() {
    const [selected, select] = useState(0);
    const offset = tokens.slice(0, selected).join("").length;
    const end = offset + tokens[selected].length;
    return (
        <div className="protocol-panel">
            <div className="protocol-head">
                <strong>www.example.com</strong>
                <span>15 байтів UTF-8 → 12 байтів Гаффмана</span>
            </div>
            <h2>Символ → код зі сталої таблиці HPACK</h2>
            <div className="character-strip">
                {Array.from(domain, (letter, i) => (
                    <button
                        key={i}
                        aria-label={`Символ ${i + 1}: ${letter}`}
                        aria-pressed={selected === i}
                        onClick={() => select(i)}
                    >
                        {letter}
                    </button>
                ))}
            </div>
            <div className="protocol-readout">
                <b>
                    «{domain[selected]}» · {tokens[selected].length} біт
                </b>
                <code>{tokens[selected]}</code>
            </div>
            <h2>Біти поспіль → байти</h2>
            <div className="packed-bits">
                {packed.map((byte, i) => (
                    <code key={i}>
                        {Array.from(byte, (bit, j) => {
                            const k = i * 8 + j;
                            return (
                                <span
                                    key={j}
                                    className={
                                        k >= bitCount
                                            ? "pad-bit"
                                            : k >= offset && k < end
                                              ? "selected-bit"
                                              : ""
                                    }
                                >
                                    {bit}
                                </span>
                            );
                        })}
                    </code>
                ))}
            </div>
            <p className="protocol-caption">
                {bitCount} біт даних + {padding} біт доповнення одиницями · H =
                1 · довжина рядка 12 Б
            </p>
        </div>
    );
}
