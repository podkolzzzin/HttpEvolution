import { hpackStaticTable } from "../data/hpack-static";

export function HpackStaticTable() {
    return (
        <details className="wire hpack-static-reference">
            <summary>Статична таблиця HPACK · усі 61 запис</summary>
            <table className="header-example">
                <caption>Вбудовані пари імен і значень за RFC 7541, додаток A</caption>
                <thead>
                    <tr>
                        <th scope="col">Індекс</th>
                        <th scope="col">Ім’я</th>
                        <th scope="col">Значення</th>
                    </tr>
                </thead>
                <tbody>
                    {hpackStaticTable.map(([name, value], offset) => (
                        <tr key={offset}>
                            <th scope="row">{offset + 1}</th>
                            <td><code>{name}</code></td>
                            <td>{value ? <code>{value}</code> : <span>порожнє</span>}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </details>
    );
}
