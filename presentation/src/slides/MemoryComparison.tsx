export function MemoryBenchmark() {
    return (
        <div className="memory-benchmark">
            <table className="memory-results">
                <caption>Жива керована пам’ять .NET на одне з’єднання</caption>
                <thead>
                    <tr>
                        <th scope="col">Протокол у демо</th>
                        <th scope="col">Після GET /</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <th scope="row">HTTP/0.9 і 1.0</th>
                        <td>
                            <small>з’єднання закрито</small>
                        </td>
                    </tr>
                    <tr>
                        <th scope="row">HTTP/1.1 keep-alive</th>
                        <td>3,63 КіБ</td>
                    </tr>
                    <tr>
                        <th scope="row">HTTP/1.1 + TLS 1.3</th>
                        <td>4,11 КіБ</td>
                    </tr>
                    <tr>
                        <th scope="row">HTTP/2</th>
                        <td>0,93 КіБ</td>
                    </tr>
                    <tr>
                        <th scope="row">HTTP/2 + TLS 1.3</th>
                        <td>1,38 КіБ</td>
                    </tr>
                    <tr>
                        <th scope="row">HTTP/3 + QUIC</th>
                        <td>647 КіБ</td>
                    </tr>
                </tbody>
            </table>
            <p>
                Медіана 9 запусків · Release, .NET 10.0.1 на Linux. Враховано
                керовану купу сервера після GC. Буфери ОС і нативна пам’ять поза
                виміром.
            </p>
            <div className="memory-links">
                <a href="./connection-memory.csv" download>Сирі вимірювання (CSV)</a>
                <a href="#server-memory">Порівняти Kestrel і nginx →</a>
            </div>
        </div>
    );
}

export function MemoryMethod() {
    return (
        <details className="wire memory-method">
            <summary>Методика й контрольні виміри</summary>
            <div>
                <p>
                    Один клієнт через loopback, GET /, 300 мс паузи. Від обсягу
                    живих об’єктів під час паузи відняли обсяг після закриття.
                    Ще один GET підтвердив, що з’єднання пережило паузу. Кожен
                    повтор запускав новий прогрітий процес.
                </p>
                <p>
                    У HTTP/1.1 залишається StreamReader із буферами. HTTP/2 у
                    демо читає кадри напряму й після короткого запиту тримає
                    мало записів HPACK. Індексація заголовка зі значенням у 4
                    000 ASCII-символів додала 7,91 КіБ керованої купи. Значення
                    зберігається як рядок .NET у UTF-16. Ліміт 4 КіБ за обліком
                    HPACK дає близько 39 МіБ на 10 000 повних таблиць.
                </p>
                <p>
                    У QUIC п’ять буферів — простір 1-RTT, керівний потік, два
                    потоки QPACK і потік запиту. Другий завершений запит на
                    новому потоці додав близько 129 КіБ. Для 8, 16 і 32 сесій
                    результат лишився близьким до 648 КіБ на сесію.
                </p>
            </div>
        </details>
    );
}

export function MemoryTimeline() {
    return (
        <figure className="memory-timeline">
            <table>
                <caption>Час життя стану на сервері</caption>
                <colgroup>
                    <col style={{ width: "24%" }} />
                    <col span={3} />
                </colgroup>
                <thead>
                    <tr>
                        <th scope="col">Протокол</th>
                        <th scope="col">Запит</th>
                        <th scope="col">Пауза</th>
                        <th scope="col">Після закриття</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <th scope="row">
                            HTTP/0.9 і 1.0<small>демо</small>
                        </th>
                        <td>
                            <span className="memory-bar">
                                TCP-сокет і буфери
                            </span>
                        </td>
                        <td></td>
                        <td></td>
                    </tr>
                </tbody>
                <tbody>
                    <tr>
                        <th scope="row">
                            HTTP/1.1<small>keep-alive</small>
                        </th>
                        <td colSpan={2}>
                            <span className="memory-bar">
                                Той самий сокет і буфери TCP
                            </span>
                        </td>
                        <td></td>
                    </tr>
                </tbody>
                <tbody>
                    <tr>
                        <th scope="row">TLS</th>
                        <td colSpan={2}>
                            <span className="memory-bar tls-state">
                                Ключі й стан шифрування
                            </span>
                        </td>
                        <td>
                            <span className="memory-bar session-cache">
                                Кеш сесій
                            </span>
                        </td>
                    </tr>
                </tbody>
                <tbody>
                    <tr>
                        <th scope="rowgroup" rowSpan={2}>
                            HTTP/2
                        </th>
                        <td colSpan={2}>
                            <span className="memory-bar">
                                Динамічні таблиці HPACK
                            </span>
                        </td>
                        <td rowSpan={2}></td>
                    </tr>
                    <tr>
                        <td>
                            <span className="memory-bar stream-state">
                                Буфери потоків
                            </span>
                        </td>
                        <td></td>
                    </tr>
                </tbody>
                <tbody>
                    <tr>
                        <th scope="rowgroup" rowSpan={2}>
                            HTTP/3
                        </th>
                        <td colSpan={2}>
                            <span className="memory-bar">
                                QUIC поверх UDP · ACK і повтори
                            </span>
                        </td>
                        <td rowSpan={2}></td>
                    </tr>
                    <tr>
                        <td colSpan={2}>
                            <span className="memory-bar stream-state">
                                Буфери завершених потоків живуть до закриття
                                сесії в демо
                            </span>
                        </td>
                    </tr>
                </tbody>
            </table>
            <figcaption>
                Пунктир — можливий кеш TLS-сесій. Демо працюють без відновлення
                TLS-сесій і з нульовою місткістю QPACK.
            </figcaption>
        </figure>
    );
}
