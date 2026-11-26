import { useState } from "react";
import { Choices } from "./ProtocolControls";
import earth from "../assets/gallery/as17-148-22727.jpg";
import moon from "../assets/gallery/PIA00405.jpg";
import saturn from "../assets/gallery/PIA11141.jpg";
import "../styles/deployment-stories.css";

const photographs = [
    { src: earth, name: "Земля" },
    { src: moon, name: "Місяць" },
    { src: saturn, name: "Сатурн" },
];

// Nine illustrative time slots. Both schedules deliver the same three JPEGs.
const galleryCheckpoints = [0, 3, 9];

function GalleryFlow({ parallel, elapsed }: { parallel: boolean; elapsed: number }) {
    const streams = photographs.map((photo, index) => {
        const previewStart = parallel ? index : index * 3;
        const detailStart = parallel ? 3 + index * 2 : previewStart + 1;
        return { ...photo, previewStart, detailStart };
    });
    return (
        <section className="gallery-flow">
            <h2>{parallel ? "Після · Cloudflare" : "До · по черзі"}</h2>
            <div className="gallery-chart" aria-hidden="true">
                {streams.map(({ name, previewStart, detailStart }) => (
                    <div className="gallery-lane" key={name}>
                        <span>{name}</span>
                        <div className="gallery-track">
                            <i className="gallery-first" style={{ gridColumn: previewStart + 1 }} />
                            <i className="gallery-detail" style={{ gridColumn: `${detailStart + 1} / span 2` }} />
                            <span className="gallery-pending" style={{ left: `${elapsed / 9 * 100}%` }} />
                        </div>
                    </div>
                ))}
            </div>
            <div className="gallery-photos">
                {streams.map(({ src, name, previewStart, detailStart }) => {
                    const full = elapsed >= detailStart + 2;
                    const visible = elapsed >= previewStart + 1;
                    return (
                        <figure key={name}>
                            {visible && (
                                <img src={src} alt="" className={full ? undefined : "gallery-preview"} />
                            )}
                            <figcaption>{name}</figcaption>
                        </figure>
                    );
                })}
            </div>
            <div className="gallery-status" aria-live="polite">
                {elapsed === 0 ? "Браузер очікує на дані."
                    : elapsed === 9 ? "Усі три фото повністю завантажені."
                    : parallel ? "Браузер показує прев’ю всіх трьох фото."
                    : "Земля вже чітка. Два інші фото ще в черзі."}
            </div>
        </section>
    );
}

export function Gallery() {
    const [stage, setStage] = useState(1);
    return (
        <div className="gallery-demo">
            <ol className="gallery-history" aria-label="Від стандарту JPEG до HTTP/2">
                <li><b>1992</b><strong>Формат JPEG</strong><span>Прогресивний режим кодує фото за кілька проходів.</span></li>
                <li><b>≈ 1996</b><strong>Браузери</strong><span>Перші проходи дають прев’ю всього кадру. Наступні додають деталі.</span></li>
                <li><b>2019</b><strong>Cloudflare</strong><span>Сервер чергує потоки HTTP/2, щоб початок кожного JPEG надійшов раніше.</span></li>
            </ol>
            <div className="gallery-controls">
                <Choices
                    name="Стан завантаження"
                    labels={["Початок", "Перші дані", "Усі байти"]}
                    selected={stage}
                    onSelect={setStage}
                />
                <div className="gallery-key">
                    <span><i className="gallery-first" />До прев’ю</span>
                    <span><i className="gallery-detail" />Деталі</span>
                    <span>Час →</span>
                </div>
            </div>
            <div className="gallery-comparison">
                <GalleryFlow parallel={false} elapsed={galleryCheckpoints[stage]} />
                <GalleryFlow parallel elapsed={galleryCheckpoints[stage]} />
            </div>
            <p>Сервер передавав готові JPEG без перекодування на льоту. У тесті Cloudflare галерею було видно раніше, а повне завантаження тривало приблизно стільки ж.</p>
            <small>Умовна шкала часу. Фото NASA, NASA/JPL/USGS та NASA/JPL/Space Science Institute.</small>
        </div>
    );
}

export function UdpStack() {
    const [choice, setChoice] = useState(1);
    const layers =
        choice === 1
            ? [
                  ["HTTP/3", "Запит і відповідь"],
                  ["QUIC + TLS 1.3", "Захист і надійні потоки"],
                  ["UDP", "Дейтаграма до порту 443"],
                  ["IP", "Шлях до вузла"],
              ]
            : [
                  ["HTTP/2", "Запит і відповідь"],
                  ["TLS", "Захищені записи"],
                  ["TCP", "Один надійний потік байтів"],
                  ["IP", "Шлях до вузла"],
              ];
    return (
        <div className="udp-stack">
            <Choices
                name="Порівняти стеки"
                labels={["HTTP/2", "HTTP/3"]}
                selected={choice}
                onSelect={setChoice}
            />
            <div className="stack-shell" aria-live="polite">
                {layers.map(([name, role], index) => (
                    <div key={name} className={"stack-layer layer-" + index}>
                        <b>{name}</b>
                        <span>{role}</span>
                    </div>
                ))}
            </div>
        </div>
    );
}

export function OperatorLimits() {
    return (
        <div className="operator-case">
            <h2>Клієнти YouTube у мережах з обмеженням швидкості UDP</h2>
            <div className="case-measure">
                <span>Червень 2015</span>
                <div>
                    <i style={{ width: "100%" }} />
                </div>
                <b>1%</b>
            </div>
            <div className="case-measure">
                <span>Листопад 2016</span>
                <div>
                    <i style={{ width: "30%" }} />
                </div>
                <b>0,3%</b>
            </div>
            <ol className="case-actions">
                <li>Google тимчасово вимикав QUIC для цієї мережі.</li>
                <li>Оператор отримував запит підняти або зняти ліміт UDP.</li>
                <li>Коли ліміт знімали, Google повертав QUIC.</li>
            </ol>
        </div>
    );
}

export function InstagramRollout() {
    const [stage, setStage] = useState(0);
    const live = stage === 1;
    return (
        <div className="insta-rollout">
            <div
                className="insta-phone"
                aria-label={
                    live
                        ? "Телефон передає ефір на сервер"
                        : "Телефон завантажує світлину зі стрічки"
                }
            >
                <div className="phone-camera" aria-hidden="true" />
                <div className="phone-image">
                    <img
                        src={live ? saturn : earth}
                        alt={live ? "Сатурн у кадрі" : "Земля у стрічці"}
                    />
                    <span>{live ? "● Прямий ефір" : "Стрічка"}</span>
                </div>
                <b>{live ? "Ефір → сервер" : "Фото ← сервер"}</b>
                <div className="phone-home" aria-hidden="true" />
            </div>
            <div className="insta-copy">
                <Choices
                    name="Етап запуску Instagram"
                    labels={["2020 · застосунки", "2026 · прямий ефір"]}
                    selected={stage}
                    onSelect={setStage}
                />
                <h2>
                    {live
                        ? "Яким шляхом піде ефір?"
                        : "Спочатку — мала частка трафіку"}
                </h2>
                <p>
                    {live
                        ? "На початку ефіру застосунок пробує QUIC і TCP. Перше успішне рукостискання визначає транспорт на весь сеанс."
                        : "Команда порівняла QUIC із попереднім транспортом на частині запитів. Після перевірки розширила запуск на Instagram для iOS та Android."}
                </p>
                <div className="insta-result">
                    {live
                        ? "Media over QUIC зрівнявся з попереднім протоколом RUSH за якістю й надійністю ефірів."
                        : "За звітом Meta, мобільні застосунки Instagram отримали виграш, порівнянний із Facebook або більший."}
                </div>
            </div>
        </div>
    );
}
