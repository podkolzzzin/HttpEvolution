import { hpackStaticTable } from "../data/hpack-static";

export const HPACK_DURATION = 36;

const CAPACITY = 160;
const FIRST_DYNAMIC_INDEX = 62;
const ENTRY_OVERHEAD = 32;

// The sample names and values below are ASCII, so character and octet counts agree.
const field = (name: string, value: string) => ({
  name,
  value,
  size: name.length + value.length + ENTRY_OVERHEAD,
});

const authority = field(":authority", "www.example.com");
const cacheControl = field("cache-control", "no-cache");
const mode = field("x-mode", "on");
const contentType = field("content-type", "application/problem+json");

const ADD_AUTHORITY = 8;
const ADD_CACHE_CONTROL = 11;
const ADD_MODE = 14;
const EVICT_AUTHORITY = 21;
const EVICT_CACHE_CONTROL = 24;
const ADD_CONTENT_TYPE = 27;
const SEND_INDEX = 30;
const READ_INDEX = 32;

const staticEntries = [1, 2, 3, 4, 24].map((index) => {
  const [name, value = "—"] = hpackStaticTable[index - 1];
  return {index, name, value};
});

const changes = [
  {at: ADD_AUTHORITY, add: authority},
  {at: ADD_CACHE_CONTROL, add: cacheControl},
  {at: ADD_MODE, add: mode},
  {at: EVICT_AUTHORITY},
  {at: EVICT_CACHE_CONTROL},
  {at: ADD_CONTENT_TYPE, add: contentType},
];

function tableAt(time: number) {
  let entries: Array<ReturnType<typeof field>> = [];
  for (const change of changes) {
    if (time < change.at) break;
    entries = change.add ? [change.add, ...entries] : entries.slice(0, -1);
  }
  return entries;
}

function StaticTable() {
  return (
    <section className="he-dictionary-section">
      <div className="he-pair">
        <h3>Статична таблиця</h3>
        <span className="he-badge" data-tone="static">
          61 запис
        </span>
      </div>
      <table className="he-table">
        <thead>
          <tr>
            <th scope="col">Індекс</th>
            <th scope="col">Назва</th>
            <th scope="col">Значення</th>
          </tr>
        </thead>
        <tbody>
          {staticEntries.map((entry) => (
            <tr key={entry.index} data-tone="static">
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
    </section>
  );
}

function progress(time: number, from: number, to: number) {
  return Math.max(0, Math.min(1, (time - from) / (to - from)));
}

function caption(time: number) {
  if (time < 5)
    return "Статична таблиця вже заповнена. Динамічна поки порожня.";
  if (time < ADD_AUTHORITY)
    return "Ім’я беремо зі статичної таблиці. Значення передаємо літералом.";
  if (time < ADD_CACHE_CONTROL)
    return "Перший динамічний запис має індекс 62, після 61 статичного.";
  if (time < ADD_MODE)
    return "Новий запис стає 62. Попередній зсувається до 63.";
  if (time < 17)
    return "Кожна вставка зверху зсуває індекси попередніх записів.";
  if (time < EVICT_AUTHORITY)
    return "Новий запис не поміщається. Першим випадає найстаріший.";
  if (time < EVICT_CACHE_CONTROL)
    return "Після першого витіснення бракує ще одного байта.";
  if (time < ADD_CONTENT_TYPE)
    return "Тепер місця достатньо. Збережений рядок лишається в таблиці.";
  if (time < SEND_INDEX)
    return "Новий запис займає 62. Збережений x-mode переходить до 63.";
  if (time < READ_INDEX)
    return "Повторний запит передає лише індекс потрібного поля.";
  return "За індексом 63 декодер відновлює x-mode = on.";
}

function Operation({
  time,
  used,
}: {
  readonly time: number;
  readonly used: number;
}) {
  if (time < 5) {
    return (
      <section className="he-card he-hpack-operation">
        <h3>Перша вставка</h3>
        <p><code>:authority = www.example.com</code></p>
      </section>
    );
  }

  if (time >= SEND_INDEX) {
    const decoded = time >= READ_INDEX;
    return (
      <section className="he-card he-hpack-operation">
        <h3>Повторний запит</h3>
        <p className="he-note" data-tone={decoded ? "done" : "live"}>
          <span>
            Індекс <strong>63</strong> · <code>0xBF</code> · 1 байт
          </span>
          {decoded && (
            <strong>
              <code>x-mode = on</code> відновлено
            </strong>
          )}
        </p>
        {!decoded && (
          <div className="he-track" aria-hidden="true">
            <span
              className="he-marker"
              style={{left: `${100 * progress(time, SEND_INDEX, READ_INDEX)}%`}}
            />
          </div>
        )}
      </section>
    );
  }

  if (time >= 17) {
    const committed = time >= ADD_CONTENT_TYPE;
    const projected = used + contentType.size;
    return (
      <section className="he-card he-hpack-operation">
        <div className="he-pair">
          <h3>{committed ? "Новий запис" : "Наступний запис"}</h3>
          {!committed && (
            <span className="he-badge" data-tone="wait">
              Поза таблицею
            </span>
          )}
        </div>
        <p className="he-pair">
          <code>{contentType.name}</code>
          <strong>{contentType.size} Б</strong>
        </p>
        <p>
          <code>{contentType.value}</code>
        </p>
        {committed ? (
          <p className="he-note" data-tone="done">
            <span>
              <code>x-mode</code> · 62 → 63
            </span>
          </p>
        ) : (
          <p
            className="he-hpack-projection"
            data-tone={projected > CAPACITY ? "wait" : "done"}
          >
            <span>Сума з кандидатом</span>
            <strong>
              {used} + {contentType.size} = {projected} Б
            </strong>
          </p>
        )}
        {time >= EVICT_AUTHORITY && !committed && (
          <p className="he-muted">
            Витіснено <code>{authority.name}</code>
            {time >= EVICT_CACHE_CONTROL && (
              <>
                {" "}
                і <code>{cacheControl.name}</code>
              </>
            )}
          </p>
        )}
      </section>
    );
  }

  if (time >= ADD_MODE) {
    return (
      <section className="he-card he-hpack-operation">
        <h3>Три динамічні записи</h3>
        <p>
          Лишилося <strong>{CAPACITY - used} Б</strong> до ліміту.
        </p>
      </section>
    );
  }

  const next =
    time < ADD_AUTHORITY
      ? authority
      : time < ADD_CACHE_CONTROL
        ? cacheControl
        : mode;
  const ends =
    time < ADD_AUTHORITY
      ? ADD_AUTHORITY
      : time < ADD_CACHE_CONTROL
        ? ADD_CACHE_CONTROL
        : ADD_MODE;
  return (
    <section className="he-card he-hpack-operation">
      <h3>Літерал з індексацією</h3>
      <p className="he-pair">
        <code>{next.name}</code>
        <strong>{next.size} Б</strong>
      </p>
      <p>
        <code>{next.value}</code>
      </p>
      <div className="he-track" aria-hidden="true">
        <span
          className="he-marker"
          style={{left: `${100 * progress(time, ends - 3, ends)}%`}}
        />
      </div>
      <p className="he-muted">Після вставки отримає індекс 62</p>
    </section>
  );
}

export function HpackDiagram({time}: {readonly time: number}) {
  const seconds = Number.isFinite(time)
    ? Math.min(HPACK_DURATION, Math.max(0, time))
    : 0;
  const entries = tableAt(seconds);
  const used = entries.reduce((total, entry) => total + entry.size, 0);
  const eviction =
    seconds >= 17 && seconds < EVICT_AUTHORITY
      ? EVICT_AUTHORITY
      : seconds >= EVICT_AUTHORITY && seconds < EVICT_CACHE_CONTROL
        ? EVICT_CACHE_CONTROL
        : null;

  return (
    <section
      className="he-compression he-hpack"
      lang="uk"
      aria-label="Анімація таблиць заголовків HPACK"
    >
      <div className="he-frame">
        <div className="he-body">
          <div className="he-card he-dictionary">
            <StaticTable />
            <section className="he-dictionary-section he-hpack-dynamic">
              <h3>Динамічна таблиця</h3>
              <div className="he-count">
                <span>Зайнято / ліміт прикладу</span>
                <strong>
                  {used} / {CAPACITY} Б
                </strong>
              </div>
              <div
                className="he-meter"
                role="meter"
                aria-label="Заповнення динамічної таблиці"
                aria-valuemin={0}
                aria-valuemax={CAPACITY}
                aria-valuenow={used}
              >
                <span style={{width: `${(100 * used) / CAPACITY}%`}} />
              </div>
              <table className="he-table he-hpack-table">
                <thead>
                  <tr>
                    <th scope="col">Індекс</th>
                    <th scope="col">Назва</th>
                    <th scope="col">Значення</th>
                    <th scope="col">Розмір</th>
                  </tr>
                </thead>
                <tbody>
                  {entries.map((entry, position) => {
                    const oldest = position === entries.length - 1;
                    const leaving = oldest && eviction !== null;
                    const exit = leaving
                      ? progress(seconds, eviction - 0.85, eviction)
                      : 0;
                    const tone = leaving
                      ? "wait"
                      : seconds >= SEND_INDEX && entry === mode
                        ? seconds >= READ_INDEX
                          ? "done"
                          : "live"
                        : position === 0
                          ? "live"
                          : undefined;
                    return (
                      <tr
                        key={entry.name}
                        data-tone={tone}
                        style={
                          exit
                            ? {
                                opacity: 1 - exit,
                                transform: `translateX(${1.25 * exit}cqw)`,
                              }
                            : undefined
                        }
                      >
                        <th scope="row" className="he-num">
                          {FIRST_DYNAMIC_INDEX + position}
                        </th>
                        <td>
                          <code>{entry.name}</code>
                        </td>
                        <td>
                          <code>
                            {entry === contentType ? (
                              <>
                                application/
                                <wbr />
                                problem+json
                              </>
                            ) : (
                              entry.value
                            )}
                          </code>
                        </td>
                        <td className="he-num">{entry.size} Б</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
              {!entries.length && (
                <p className="he-hpack-empty he-muted">Записів ще немає</p>
              )}
              <p className="he-muted he-hpack-footnote">
                Розмір = байти назви + байти значення + 32 Б
              </p>
              {entries.length > 1 && (
                <p className="he-muted he-hpack-footnote">
                  Найстаріший запис унизу
                </p>
              )}
            </section>
          </div>
          <Operation time={seconds} used={used} />
        </div>
        <footer className="he-footer" role="status">{caption(seconds)}</footer>
      </div>
    </section>
  );
}
