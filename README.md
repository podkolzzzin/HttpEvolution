# HttpEvolution

Від HTTP/0.9 до HTTP/3 у **30 навчальних проєктах на C#**.
Префікс папки задає порядок проходження, далі вказано рік і тему.
Номери збережено після об’єднання споріднених прикладів. У кожному проєкті свій `Program.cs`.
Незмінні та попередні версії допоміжних класів підключені як linked files.
Відкривай `HttpEvolution.slnx` або `HttpEvolution.csproj` у папці потрібного етапу.

Мережеві API — тільки `TcpListener`, `TcpClient`, `UdpClient` і потоки байтів.
TLS, QUIC, HPACK і QPACK написані тут; криптографічні примітиви — з .NET BCL.
Аргументи серверних демо обробляє NuGet-пакет `System.CommandLine`.
Потрібен **.NET 10 SDK**; усі режими слухають лише loopback.

Форматування C# закріплено в [`.editorconfig`](.editorconfig): 4 пробіли, дужки
на окремих рядках, блоки для умов і циклів, окремі оголошення та оператори.
Перевірка: `dotnet format HttpEvolution.slnx --verify-no-changes --no-restore`.

## HTML-презентація

[presentation/index.html](presentation/index.html) — один автономний HTML-файл:
63 слайди як історія розвитку HTTP: світ навколо, прорив кожної версії,
її обмеження й причина наступного кроку. 25 слайдів без коду дають контекст
та пояснюють компроміси; решта містять короткі фрагменти й команди.
Є схеми та посилання на відповідні папки й файли коду. Працює без CDN; можна відкрити файл у браузері. На телефоні — свайпи,
кнопки навігації та зміст. Є моделі перевірки Host і втрати пакета в TCP/QUIC.
SSE (чернетка 2009) показує події в HTTP-відповіді та клієнт EventSource;
WebSocket (API 2009 → RFC 2011) — Upgrade, кадри й двосторонній обмін.
Обидва приклади можна [запустити локально](15b_2011_HTTP1.1_REALTIME/) і відкрити в браузері.
Історія SSL починається з потреб Netscape, невипущеної SSL1 та недоліків SSL2;
окремо пояснено, чому реалізація демо починається із SSL3.
SSL3 розібрано на 8 кроків: алгоритми, повідомлення, RSA, master, ключі,
Finished, MAC і CBC-запис — із поясненням констант та розмірів.
Інтерактивна схема SSL порівнює відкритий HTTP із захищеним записом:
видимі IP/TCP/SSL-заголовки та зашифровані HTTP, MAC і доповнення.
Поля можна натискати; вміст запису розкривається у вигляді на сервері.
На слайді Host схема порівнює HTTP, HTTPS без ECH і HTTPS з ECH:
два послідовні повідомлення від браузера до сервера, окремо — те, що
читає провайдер. Відкритий домен виділено в SNI; Host захищено всередині
HTTP-запиту. ECH розшифровано й пояснено безпосередньо біля перемикача.
Примітка з джерелами розрізняє ESNI (2018), впровадження ECH (2023)
і стандарт RFC 9849 (березень 2026).
Інтерактивний слайд HPACK показує поруч HEX-байти HTTP/1.1 та HPACK у HTTP/2. Над кожним байтом видно відповідний символ або значення поля. Натискання підсвічує відповідне поле, а перемикач показує перший і повторний запити до www.example.com.
Підсумковий слайд про пам’ять порівнює час життя сокетів, стан TLS і QUIC,
таблиці заголовків та витрати на активні потоки.
Повний перший сервер доступний на слайді HTTP/0.9.
[Живий перегляд через Tailscale](https://jm.tail65de18.ts.net/http-demo/).

## Показати перший крок

```sh
dotnet run --project 01_1991_HTTP0.9_GET
# В іншому терміналі:
printf 'GET /\r\n' | nc 127.0.0.1 8080
# <h1>Hello, HTTP/0.9!</h1>
```

HTTP/0.9: один рядок GET; відповідь — HTML без статусу і заголовків,
після неї сервер закриває TCP-з'єднання. `/missing` повертає HTML «Not found».

## Пройти історію

Переходь між папками за номером; перемикати Git не потрібно.
У кожній папці є короткий README із запуском і порівнянням із попереднім кроком.
Наприклад, із кореня репозиторію:

```sh
dotnet run --project 09_1996_HTTP1.0_SSL3_CLIENT -- --ssl3
diff -ru --exclude=bin --exclude=obj --exclude=README.md 02_1996_HTTP1.0_HEADERS 03_1996_HTTP1.0_HEAD
```

Код завершення `diff` 1 означає, що відмінності знайдено. Для решти етапів
README містить команди порівняння фактичних C#-файлів, з урахуванням linked files.
Фізичний склад папок відображає нові версії та їхні підключення.

У кожного етапу свій фізичний `Program.cs`: `dotnet run` одразу запускає його демо.
Підготовчі етапи показують роботу класів у пам’яті й завершуються; готові сервери
слухають loopback до Ctrl+C. `Program.cs` ніколи не підключається з іншого етапу.

Незмінні допоміжні файли зібрані в [`common/`](common/). Кожна історична версія
іншого класу зберігається один раз — у першому етапі, де з’явився цей код.
Наступні `.csproj` підключають саме її через `Compile Include` та `Link`, наприклад:

```xml
<Compile Include="../21_2015_HTTP2_CONTINUATION/Http2ContinuationConnection.cs" Link="Http2ContinuationConnection.cs" />
```

В IDE кожен проєкт показує всі свої файли. Посилання ведуть безпосередньо до
потрібної версії C#-файлу; посилань із ранніх етапів на пізні немає. Для запуску
потрібен checkout репозиторію з `common/` та папками етапів.
Щоб додати наступну історичну зміну, створи нову версію лише потрібного файлу
в новому етапі й онови його `.csproj`. Попередня версія залишається на місці.

Спільний потік `TlsStream` і створення `TlsCertificate` винесені в `common`;
TLS1.3 handshake та SPKI pin додаються окремими частинами `partial` на своїх етапах.
`Http2Frames` містить однакові для всіх версій читання й запис кадрів.
Етапи HTTP/2 послідовно додають обробку кадрів (16), заголовки й відповіді
потоків (19), керування потоком (20), CONTINUATION (21) та TLS з ALPN (29).
README кожного етапу пояснює його демо та показує потрібні клієнтські команди.
У демо з прапорцями `dotnet run -- --help` показує доступні режими.
Останній етап за замовчуванням запускає HTTP/3; усі попередні готові режими
також доступні через прапорці.

Рік — історичний орієнтир протоколу або можливості, не дата написання коду.
HTTP3 у папках 2021 року позначає напрям наступного етапу: там будується QUIC v1,
на якому працює HTTP/3 (2022). HTTPS — HTTP у захищеному каналі;
SSL/TLS зазначено в короткій назві, а HTTP-префікс показує поточний етап HTTP.

| № | Папка | Що додається |
| --- | --- | --- |
| 01 | [01_1991_HTTP0.9_GET](01_1991_HTTP0.9_GET/) | GET → HTML → закриття TCP |
| 02 | [02_1996_HTTP1.0_HEADERS](02_1996_HTTP1.0_HEADERS/) | Статус, заголовки та Content-Length |
| 03 | [03_1996_HTTP1.0_HEAD](03_1996_HTTP1.0_HEAD/) | HEAD: заголовки без тіла відповіді |
| 04 | [04_1996_HTTP1.0_SSL3_PRIMITIVES](04_1996_HTTP1.0_SSL3_PRIMITIVES/) | SSL3: шифрування записів, handshake, сертифікат і потік байтів |
| 08 | [08_1996_HTTP1.0_SSL3_HTTPS](08_1996_HTTP1.0_SSL3_HTTPS/) | HTTPS через власний SSL3 handshake |
| 09 | [09_1996_HTTP1.0_SSL3_CLIENT](09_1996_HTTP1.0_SSL3_CLIENT/) | SSL3: TcpClient із перевіркою SPKI pin |
| 10 | [10_1997_HTTP1.1_HOST](10_1997_HTTP1.1_HOST/) | Обов’язковий Host для HTTP/1.1 |
| 11 | [11_1997_HTTP1.1_KEEP_ALIVE](11_1997_HTTP1.1_KEEP_ALIVE/) | Кілька запитів через одне TCP-з’єднання |
| 12 | [12_1997_HTTP1.1_CHUNKED](12_1997_HTTP1.1_CHUNKED/) | Передавання відповіді частинами: chunked |
| 13 | [13_1999_HTTP1.1_TLS10](13_1999_HTTP1.1_TLS10/) | TLS1.0: PRF, HMAC і новий Finished |
| 14 | [14_2002_HTTP1.1_TLS_AES](14_2002_HTTP1.1_TLS_AES/) | TLS1.0: AES-CBC замість 3DES |
| 15 | [15_2010_HTTP1.1_TLS_RENEGOTIATION](15_2010_HTTP1.1_TLS_RENEGOTIATION/) | TLS: renegotiation indication |
| 15b | [15b_2011_HTTP1.1_REALTIME](15b_2011_HTTP1.1_REALTIME/) | SSE у потоці HTTP-відповіді та WebSocket після Upgrade |
| 16 | [16_2015_HTTP2_FRAMES](16_2015_HTTP2_FRAMES/) | HTTP/2: preface, SETTINGS і кадри через TCP |
| 17 | [17_2015_HTTP2_HPACK](17_2015_HTTP2_HPACK/) | HPACK: таблиці заголовків і кодування рядків Huffman |
| 19 | [19_2015_HTTP2_STREAMS](19_2015_HTTP2_STREAMS/) | HTTP/2: відповіді GET/HEAD у потоках |
| 20 | [20_2015_HTTP2_FLOW_CONTROL](20_2015_HTTP2_FLOW_CONTROL/) | HTTP/2: вікна flow control |
| 21 | [21_2015_HTTP2_CONTINUATION](21_2015_HTTP2_CONTINUATION/) | HTTP/2: збирання блоків CONTINUATION |
| 22 | [22_2018_HTTP2_TLS13_KEY_EXCHANGE](22_2018_HTTP2_TLS13_KEY_EXCHANGE/) | TLS 1.3: ClientHello, обмін ключами P-256 і ключі трафіку |
| 25 | [25_2018_HTTP2_TLS13_AUTH_RECORDS](25_2018_HTTP2_TLS13_AUTH_RECORDS/) | TLS 1.3: сертифікат, Finished і захищені записи |
| 28 | [28_2018_HTTP2_TLS13_HTTPS](28_2018_HTTP2_TLS13_HTTPS/) | HTTPS через власний потік TLS1.3 |
| 29 | [29_2018_HTTP2_ALPN](29_2018_HTTP2_ALPN/) | HTTP/2 через TLS1.3 з ALPN h2 |
| 30 | [30_2021_HTTP3_QUIC_PRIMITIVES](30_2021_HTTP3_QUIC_PRIMITIVES/) | QUIC v1: varint, формат і захист пакета, зміщення, ACK і транспортні параметри |
| 36 | [36_2021_HTTP3_QUIC_TLS](36_2021_HTTP3_QUIC_TLS/) | QUIC: TLS handshake на рівнях шифрування |
| 37 | [37_2021_HTTP3_QUIC_UDP](37_2021_HTTP3_QUIC_UDP/) | QUIC: приймання сеансів через UdpClient |
| 38 | [38_2021_HTTP3_QUIC_PTO](38_2021_HTTP3_QUIC_PTO/) | QUIC: облік ACK та probe timeout |
| 39 | [39_2022_HTTP3_QPACK](39_2022_HTTP3_QPACK/) | HTTP/3: статичне кодування заголовків QPACK |
| 40 | [40_2022_HTTP3_QUIC_STREAMS](40_2022_HTTP3_QUIC_STREAMS/) | QUIC для HTTP/3: потоки й flow control |
| 41 | [41_2022_HTTP3_RESPONSE](41_2022_HTTP3_RESPONSE/) | HTTP/3: GET/HEAD через QUIC, HEADERS/DATA/FIN |
| 42 | [42_2022_HTTP3_QUIC_RECOVERY](42_2022_HTTP3_QUIC_RECOVERY/) | QUIC для HTTP/3: повтори з новими packet number |

SSL2 згаданий як попередник SSL3; його реалізації немає. TLS1.1/1.2 також
пропущені: це вибрані навчальні кроки, а не повна реалізація всіх версій.

## Повна версія

Усі команди нижче запускаються з кореня репозиторію та використовують
останній етап. Для раннього етапу заміни шлях після `--project`.
Команди сервера і клієнта виконуються в різних терміналах.

## HTTP/1.x

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --http
curl --http1.0 -i http://127.0.0.1:8080/
curl --http1.0 -I http://127.0.0.1:8080/
curl --http1.1 --raw -i http://127.0.0.1:8080/chunks
```

Команди сервера і клієнта запускаються в різних терміналах.
GET/HEAD, `/`, `/chunks`, 404 для інших шляхів. HTTP/1.1 вимагає єдиного
непорожнього Host, повторно використовує TCP; `Connection: close` завершує сеанс.
На `/chunks` видно дві частини і кінцевий нуль. Ненульове тіло запиту
відхиляється з 400. Один TCP-клієнт за раз, таймаут 5 секунд.

### SSE та WebSocket

Запусти один із двох режимів, а потім відкрий `http://127.0.0.1:8082/` у браузері.

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --sse
# В іншому терміналі можна побачити події у форматі text/event-stream:
curl -N http://127.0.0.1:8082/events
```

Зупини сервер через Ctrl+C, запусти WebSocket і перезавантаж сторінку. Браузер надішле «Привіт!» і покаже відповідь сервера.

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --ws
```

## Історичні SSL3 / TLS1.0

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --ssl3
# В іншому терміналі підстав надрукований SPKI pin:
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --ssl3-client PIN

# Аналогічно для TLS1.0:
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --tls10
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --tls10-client PIN
```

SSL3 слухає TCP:8442, TLS1.0 — TCP:8441. Власний `TcpClient` перевіряє pin
сертифіката і Finished; ці старі протоколи використовуються лише як історичне демо.
Сертифікат тимчасовий, RSA/SHA-1; приватний ключ існує лише в пам'яті.
TLS1.0 також приймає незалежний OpenSSL-клієнт із AES128-SHA:

```sh
printf 'GET / HTTP/1.0\r\n\r\n' | openssl s_client -quiet -tls1 -cipher 'AES128-SHA:@SECLEVEL=0' -connect 127.0.0.1:8441
```

В окремих папках додано AES (2002) і початкову renegotiation indication
(2010). Повторного handshake немає. Сучасний режим використовує TLS1.3:

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --tls
curl -k --http1.1 -i https://localhost:8443/
```

TLS1.3: AES-128-GCM-SHA256, P-256, ECDSA, ALPN, HelloRetryRequest.
`-k` допускає тимчасовий self-signed сертифікат демо; resumption/0-RTT/KeyUpdate немає.

## HTTP/2

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --h2
curl --http2-prior-knowledge -i http://127.0.0.1:8081/
# Або HTTP/2 через TLS1.3, ALPN h2:
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --h2 --tls
curl -k --http2 -I https://localhost:8444/
```

Потрібен curl із підтримкою HTTP/2. Preface, SETTINGS/ACK, PING, GET/HEAD,
непарні stream ID, HPACK із динамічною таблицею і Huffman, CONTINUATION,
WINDOW_UPDATE та RST_STREAM. Тіло запиту не підтримується.

## HTTP/3

```sh
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY -- --h3
# В іншому терміналі підстав надрукований SPKI pin:
google-chrome --user-data-dir="$(mktemp -d)" --origin-to-force-quic-on=127.0.0.1:8445 --ignore-certificate-errors-spki-list=PIN https://127.0.0.1:8445/
```

UDP:8445; QUIC v1, TLS1.3/ALPN h3, CRYPTO/STREAM, control SETTINGS,
статичний QPACK і Huffman, GET/HEAD, HEADERS/DATA/FIN. Динамічну місткість QPACK
узгоджено як 0. Є ACK-діапазони, захист від amplification, flow control,
повторна передача з новим packet number, PTO/backoff і видалення старих ключів.

Це локальна модель: до 16 bidi та 16 uni потоків, 64 KiB на receive-потік,
1 MiB на з'єднання. Вікно надсилання — до 12 000 байтів; повного adaptive RTT
і congestion control з RFC9002, migration та 0-RTT немає.

## Перевірено

Усі 30 проєктів збираються в Release без попереджень.
Зібрати всі етапи: `dotnet build HttpEvolution.slnx -c Release`.
HTTP/1.x та HTTP/2 — curl і сирі кадри, включно з вікнами й скасуванням потоків.
TLS1.0 та TLS1.3 — незалежний OpenSSL, перевірка сертифіката/hostname і close_notify;
SSL3 — власний pinned peer та незалежні контрольні значення криптографії.
TLS/QUIC/HPACK/QPACK побайтово звірені з прикладами RFC.
Chrome відобразив сторінку через HTTP/3, зокрема після втрати Initial,
Handshake і 1-RTT пакетів та дублювання Initial. Це демо підмножини протоколів,
не сервер для production.

Першоджерела: [HTTP/0.9](https://www.w3.org/Protocols/HTTP/AsImplemented),
[HTTP/1.0](https://www.rfc-editor.org/rfc/rfc1945),
[HTTP/1.1](https://www.rfc-editor.org/rfc/rfc2068),
[SSL3](https://www.rfc-editor.org/rfc/rfc6101),
[TLS1.0](https://www.rfc-editor.org/rfc/rfc2246),
[AES у TLS](https://www.rfc-editor.org/rfc/rfc3268),
[renegotiation indication](https://www.rfc-editor.org/rfc/rfc5746),
[HTTP/2 + HPACK](https://www.rfc-editor.org/rfc/rfc7540),
[TLS1.3](https://www.rfc-editor.org/rfc/rfc8446),
[TLS приклади](https://www.rfc-editor.org/rfc/rfc8448),
[QUIC](https://www.rfc-editor.org/rfc/rfc9000),
[QUIC crypto-приклади](https://www.rfc-editor.org/rfc/rfc9001),
[HTTP/3](https://www.rfc-editor.org/rfc/rfc9114),
[QPACK](https://www.rfc-editor.org/rfc/rfc9204).
