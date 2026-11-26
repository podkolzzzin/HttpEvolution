import "./QuicReliability.css";

const rows = [
    { label: "Пакет 17", offset: 0, data: "ABCD" },
    { label: "Пакет 24", offset: 2, data: "CDEF" },
    { label: "Застосунок", offset: 0, data: "ABCDEF" },
];

export function QuicReliability() {
    return (
        <figure className="qw quic-overlap">
            <table>
                <caption>Два кадри STREAM одного потоку</caption>
                <thead>
                    <tr>
                        <th scope="col">Зсув</th>
                        {Array.from({ length: 6 }, (_, i) => <th key={i} scope="col">{i}</th>)}
                    </tr>
                </thead>
                <tbody>
                    {rows.map(({ label, offset, data }, row) => (
                        <tr key={label}>
                            <th scope="row">{label}</th>
                            {Array.from({ length: 6 }, (_, i) => {
                                const byte = data[i - offset];
                                const repeated = row === 1 && (i === 2 || i === 3);
                                return (
                                    <td key={i} className={repeated ? "repeated" : byte ? "filled" : undefined}>
                                        {byte ?? "·"}
                                    </td>
                                );
                            })}
                        </tr>
                    ))}
                </tbody>
            </table>
            <figcaption>
                ACK затримався. Пакет 24 приніс повторні <b>CD</b> разом із новими <b>EF</b>.
                Це перекриття на зсувах 2 і 3. Застосунок прочитає <b>ABCDEF</b>.
            </figcaption>
        </figure>
    );
}
