import type {ReactNode} from "react";

export const QPACK_DURATION = 31;

const INTRO_END = 5;
const A_SEND = 7.5;
const A_ARRIVAL = 10;
const B_SEND = 9.5;
const B_ARRIVAL = 13;

const B_REQUIRED_INSERT_COUNT = 0;

const getMethod = {index: 17, name: ":method", value: "GET"} as const;
const getPath = {index: 1, name: ":path", value: "/"} as const;
const staticEntries = [
  {index: 0, name: ":authority", value: "—"},
  getPath,
  getMethod,
  {index: 20, name: ":method", value: "POST"},
  {index: 23, name: ":scheme", value: "https"},
] as const;

const formatEntry = {
  index: 2,
  name: "x-format",
  value: "compact",
  send: 7,
  arrive: 24,
} as const;

const dynamicEntries = [
  {index: 0, name: ":authority", value: "www.example.com", send: 0, arrive: 0},
  {index: 1, name: "x-region", value: "eu", send: 6, arrive: 17},
  formatEntry,
] as const;

const encoderCommands = dynamicEntries.slice(1);
// All three dynamic references start at absolute index 0 and are never evicted here.
const requiredInsertCount = dynamicEntries.length;
const fields = [
  {...getMethod, origin: "Стат. №", arrive: 0},
  ...dynamicEntries.map((entry) => ({...entry, origin: "Абс. №"})),
];

function Table({
  rows,
  isStatic = false,
}: {
  readonly rows: ReadonlyArray<{
    readonly index: number;
    readonly name: string;
    readonly value: string;
  }>;
  readonly isStatic?: boolean;
}) {
  return (
    <table className="he-table">
      <thead>
        <tr>
          <th scope="col">{isStatic ? "Індекс" : "Абс. №"}</th>
          <th scope="col">Назва</th>
          <th scope="col">Значення</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((entry) => (
          <tr key={entry.index} data-tone={isStatic ? "static" : "live"}>
            <th scope="row" className="he-num">
              {entry.index}
            </th>
            <td>
              <code>{entry.name}</code>
            </td>
            <td>
              <code>{entry.value}</code>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function TransferTrack({
  time,
  send,
  arrive,
  kind,
  children,
}: {
  readonly time: number;
  readonly send: number;
  readonly arrive: number;
  readonly kind: "insert" | "headers";
  readonly children: ReactNode;
}) {
  const progress = Math.max(0, Math.min(1, (time - send) / (arrive - send)));
  const position = `${progress * 100}%`;
  return (
    <div className="he-qpack-track">
      <span
        className="he-qpack-packet"
        data-kind={kind}
        style={{
          insetInlineStart: position,
          transform: `translateX(-${position})`,
        }}
      >
        {children}
      </span>
    </div>
  );
}

function DecoderCheck({
  insertCount,
  required,
  arrived,
}: {
  readonly insertCount: number;
  readonly required: number;
  readonly arrived: boolean;
}) {
  const ready = insertCount >= required;
  return (
    <span
      className="he-badge he-qpack-check"
      data-tone={arrived ? (ready ? "done" : "wait") : "neutral"}
    >
      {arrived ? (
        <>
          Декодер{" "}
          <strong>
            {insertCount} {ready ? "≥" : "<"} {required}
          </strong>
        </>
      ) : (
        <>Потрібно вставок: {required}</>
      )}
    </span>
  );
}

function RequestA({
  time,
  canDecode,
  insertCount,
}: {
  readonly time: number;
  readonly canDecode: boolean;
  readonly insertCount: number;
}) {
  const arrived = time >= A_ARRIVAL;
  const available = fields.filter((field) => time >= field.arrive).length;

  return (
    <section className="he-stream he-qpack-a">
      <div className="he-pair">
        <h3>Потік запиту A</h3>
        <span
          className="he-badge"
          data-tone={canDecode ? "done" : arrived ? "wait" : "neutral"}
        >
          {canDecode
            ? "Декодовано цілком"
            : arrived
              ? "Секція чекає"
              : time >= A_SEND
                ? "У дорозі"
                : "У черзі"}
        </span>
      </div>
      <TransferTrack
        time={time}
        send={A_SEND}
        arrive={A_ARRIVAL}
        kind="headers"
      >
        <b>HEADERS</b>
        <code>{formatEntry.name}: {formatEntry.value}</code>
      </TransferTrack>
      <p className="he-count">
        <DecoderCheck
          insertCount={insertCount}
          required={requiredInsertCount}
          arrived={arrived}
        />
        <span>
          Записи{" "}
          <strong>
            {available} з {fields.length}
          </strong>
        </span>
      </p>
      <ul className="he-ref-list">
        {fields.map((field) => {
          const present = time >= field.arrive;
          return (
            <li
              key={field.name}
              className="he-ref-row"
              data-tone={canDecode ? "done" : present ? "static" : "wait"}
            >
              <span>
                <small>
                  {field.origin} {field.index}
                </small>{" "}
                <code>{field.name}</code>
              </span>
              {canDecode ? (
                <code>{field.value}</code>
              ) : (
                <strong>{present ? "Є запис" : "Бракує"}</strong>
              )}
            </li>
          );
        })}
      </ul>
    </section>
  );
}

function RequestB({
  time,
  insertCount,
}: {
  readonly time: number;
  readonly insertCount: number;
}) {
  const arrived = time >= B_ARRIVAL;
  const decoded = arrived && insertCount >= B_REQUIRED_INSERT_COUNT;
  return (
    <section className="he-stream he-qpack-b">
      <div className="he-pair">
        <h3>Потік запиту B</h3>
        <span className="he-badge" data-tone={decoded ? "done" : "neutral"}>
          {decoded ? "Декодовано" : time >= B_SEND ? "У дорозі" : "У черзі"}
        </span>
      </div>
      <TransferTrack
        time={time}
        send={B_SEND}
        arrive={B_ARRIVAL}
        kind="headers"
      >
        <b>HEADERS</b>
        <code>{getMethod.name}: {getMethod.value}</code>
        <code>{getPath.name}: {getPath.value}</code>
      </TransferTrack>
      <p className="he-count">
        <DecoderCheck
          insertCount={insertCount}
          required={B_REQUIRED_INSERT_COUNT}
          arrived={arrived}
        />
        {decoded && (
          <code>
            {getMethod.name} {getMethod.value} · {getPath.name} {getPath.value}
          </code>
        )}
      </p>
    </section>
  );
}

function Encoder({time}: {readonly time: number}) {
  return (
    <section className="he-stream he-qpack-encoder">
      <h3>Вставки в потоці кодера</h3>
      {encoderCommands.map((entry) => {
        const delivered = time >= entry.arrive;
        const traveling = time >= entry.send && !delivered;
        return (
          <div className="he-qpack-command" key={entry.index}>
            <TransferTrack
              time={time}
              send={entry.send}
              arrive={entry.arrive}
              kind="insert"
            >
              <code>
                {entry.name}: {entry.value}
              </code>
            </TransferTrack>
            <span
              className="he-badge"
              data-tone={delivered ? "done" : traveling ? "live" : "neutral"}
            >
              {delivered ? "Доставлено" : traveling ? "У дорозі" : "У черзі"}
            </span>
          </div>
        );
      })}
    </section>
  );
}

function caption(time: number, canDecode: boolean, insertCount: number) {
  if (time < INTRO_END)
    return "Статичні записи готові. Динамічний запис № 0 уже надійшов.";
  if (time < A_ARRIVAL)
    return "Запити й команди вставки йдуть окремими QUIC-потоками.";
  if (canDecode) return "Декодер додав x-format. Тепер він може прочитати всі заголовки запиту A.";
  if (insertCount === 2) return "Декодер додав x-region. Запит A ще чекає на x-format.";
  if (time >= B_ARRIVAL)
    return "B декодується за статичними записами, поки A чекає.";
  return "A надійшла раніше за обидві потрібні вставки.";
}

export function QpackDiagram({time}: {readonly time: number}) {
  const seconds = Number.isFinite(time)
    ? Math.min(QPACK_DURATION, Math.max(0, time))
    : 0;
  const received = dynamicEntries.filter((entry) => seconds >= entry.arrive);
  const canDecode =
    seconds >= A_ARRIVAL && received.length >= requiredInsertCount;

  return (
    <section
      className="he-compression he-qpack"
      lang="uk"
      aria-label="Анімація таблиць заголовків QPACK"
    >
      <div className="he-frame">
        <div className="he-body he-qpack-body">
          <div className="he-card he-dictionary">
            <section className="he-dictionary-section">
              <h3>Статична таблиця</h3>
              <p className="he-muted">Вибрані записи RFC 9204 · 99 записів</p>
              <Table rows={staticEntries} isStatic />
            </section>
            <section className="he-dictionary-section he-qpack-table">
              <h3>Динамічна таблиця декодера</h3>
              <p className="he-qpack-formula">
                <strong>Insert Count ≥ Required Insert Count</strong>
              </p>
              <p className="he-muted">
                Оброблено вставок <strong>{received.length}</strong>
              </p>
              <Table rows={received} />
            </section>
          </div>
          <div
            className="he-card he-qpack-streams"
            role="group"
            aria-label="Потоки QUIC"
          >
            <Encoder time={seconds} />
            <RequestA
              time={seconds}
              canDecode={canDecode}
              insertCount={received.length}
            />
            <RequestB time={seconds} insertCount={received.length} />
          </div>
        </div>
        <footer className="he-footer" role="status">
          {caption(seconds, canDecode, received.length)}
        </footer>
      </div>
    </section>
  );
}
