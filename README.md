# HttpEvolution

Навчальне C#-демо: від HTTP/0.9 до HTTP/3 у **42 послідовних папках**.
Кожна папка має формат `NN_<year>_HTTP<version>_<SHORT_NAME>`
і містить окремий виконуваний проєкт: попередній код + одна невелика можливість.
Відкривай `HttpEvolution.slnx` або `HttpEvolution.csproj` у папці потрібного етапу.

Мережеві API — тільки `TcpListener`, `TcpClient`, `UdpClient` і потоки байтів.
TLS, QUIC, HPACK і QPACK написані тут; криптографічні примітиви — з .NET BCL.
Без NuGet-залежностей, ASP.NET, `HttpListener`, `SslStream` і `System.Net.Quic`.
Потрібен **.NET 10 SDK**; усі режими слухають лише loopback.

## HTML-презентація

[presentation/index.html](presentation/index.html) — один автономний HTML-файл:
59 слайдів як історія розвитку HTTP: світ навколо, прорив кожної версії,
її обмеження й причина наступного кроку. 22 слайди без коду дають контекст
та пояснюють компроміси; решта містять короткі фрагменти й команди.
Є схеми та посилання на відповідні папки й файли коду. Працює без CDN; можна відкрити файл у браузері. На телефоні — свайпи,
кнопки навігації та зміст. Є моделі перевірки Host і втрати пакета в TCP/QUIC.
SSE (чернетка 2009) показує події в HTTP-відповіді та клієнт EventSource;
WebSocket (API 2009 → RFC 2011) — Upgrade, кадри й двосторонній обмін.
Це навчальні приклади формату обміну з браузерним JavaScript.
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

`diff` порівнює код і файли проєктів, пропускаючи `bin/`, `obj/` та README;
код завершення 1 означає, що відмінності знайдено. Незмінні допоміжні файли
зібрані в [`common/`](common/) і явно підключені до потрібних `.csproj` як linked files.
Підготовчі етапи додають класи; їхній README пояснює, коли новий протокол
стає доступним для запитів.

Рік — історичний орієнтир протоколу або можливості, не дата написання коду.
HTTP3 у папках 2021 року позначає напрям наступного етапу: там будується QUIC v1,
на якому працює HTTP/3 (2022). HTTPS — HTTP у захищеному каналі;
SSL/TLS зазначено в короткій назві, а HTTP-префікс показує поточний етап HTTP.

| № | Папка | Що додається |
| --- | --- | --- |
| 01 | [01_1991_HTTP0.9_GET](01_1991_HTTP0.9_GET/) | GET → HTML → закриття TCP |
| 02 | [02_1996_HTTP1.0_HEADERS](02_1996_HTTP1.0_HEADERS/) | Статус, заголовки та Content-Length |
| 03 | [03_1996_HTTP1.0_HEAD](03_1996_HTTP1.0_HEAD/) | HEAD: заголовки без тіла відповіді |
| 04 | [04_1996_HTTP1.0_SSL3_RECORDS](04_1996_HTTP1.0_SSL3_RECORDS/) | SSL3: MAC і шифрування записів 3DES-CBC |
| 05 | [05_1996_HTTP1.0_SSL3_MESSAGES](05_1996_HTTP1.0_SSL3_MESSAGES/) | SSL3: повідомлення handshake та ChangeCipherSpec |
| 06 | [06_1996_HTTP1.0_SSL3_IDENTITY](06_1996_HTTP1.0_SSL3_IDENTITY/) | SSL3: сертифікат RSA/SHA-1 |
| 07 | [07_1996_HTTP1.0_SSL3_STREAM](07_1996_HTTP1.0_SSL3_STREAM/) | SSL3: розшифровані записи як потік байтів |
| 08 | [08_1996_HTTP1.0_SSL3_HTTPS](08_1996_HTTP1.0_SSL3_HTTPS/) | HTTPS через власний SSL3 handshake |
| 09 | [09_1996_HTTP1.0_SSL3_CLIENT](09_1996_HTTP1.0_SSL3_CLIENT/) | SSL3: TcpClient із перевіркою SPKI pin |
| 10 | [10_1997_HTTP1.1_HOST](10_1997_HTTP1.1_HOST/) | Обов’язковий Host для HTTP/1.1 |
| 11 | [11_1997_HTTP1.1_KEEP_ALIVE](11_1997_HTTP1.1_KEEP_ALIVE/) | Кілька запитів через одне TCP-з’єднання |
| 12 | [12_1997_HTTP1.1_CHUNKED](12_1997_HTTP1.1_CHUNKED/) | Передавання відповіді частинами: chunked |
| 13 | [13_1999_HTTP1.1_TLS10](13_1999_HTTP1.1_TLS10/) | TLS1.0: PRF, HMAC і новий Finished |
| 14 | [14_2002_HTTP1.1_TLS_AES](14_2002_HTTP1.1_TLS_AES/) | TLS1.0: AES-CBC замість 3DES |
| 15 | [15_2010_HTTP1.1_TLS_RENEGOTIATION](15_2010_HTTP1.1_TLS_RENEGOTIATION/) | TLS: renegotiation indication |
| 16 | [16_2015_HTTP2_FRAMES](16_2015_HTTP2_FRAMES/) | HTTP/2: preface, SETTINGS і кадри через TCP |
| 17 | [17_2015_HTTP2_HPACK](17_2015_HTTP2_HPACK/) | HPACK: таблиці заголовків |
| 18 | [18_2015_HTTP2_HUFFMAN](18_2015_HTTP2_HUFFMAN/) | HPACK: декодування рядків Huffman |
| 19 | [19_2015_HTTP2_STREAMS](19_2015_HTTP2_STREAMS/) | HTTP/2: відповіді GET/HEAD у потоках |
| 20 | [20_2015_HTTP2_FLOW_CONTROL](20_2015_HTTP2_FLOW_CONTROL/) | HTTP/2: вікна flow control |
| 21 | [21_2015_HTTP2_CONTINUATION](21_2015_HTTP2_CONTINUATION/) | HTTP/2: збирання блоків CONTINUATION |
| 22 | [22_2018_HTTP2_TLS13_KEYS](22_2018_HTTP2_TLS13_KEYS/) | TLS1.3: ключі трафіку та захист записів |
| 23 | [23_2018_HTTP2_TLS13_HELLO](23_2018_HTTP2_TLS13_HELLO/) | TLS1.3: ClientHello та P-256 key share |
| 24 | [24_2018_HTTP2_TLS13_ECDH](24_2018_HTTP2_TLS13_ECDH/) | TLS1.3: узгодження секретів ECDH/P-256 |
| 25 | [25_2018_HTTP2_TLS13_IDENTITY](25_2018_HTTP2_TLS13_IDENTITY/) | TLS1.3: сертифікат localhost і підписи ECDSA |
| 26 | [26_2018_HTTP2_TLS13_FINISHED](26_2018_HTTP2_TLS13_FINISHED/) | TLS1.3: автентифікація та Finished |
| 27 | [27_2018_HTTP2_TLS13_RECORDS](27_2018_HTTP2_TLS13_RECORDS/) | TLS1.3: формат і автентифікація записів |
| 28 | [28_2018_HTTP2_TLS13_HTTPS](28_2018_HTTP2_TLS13_HTTPS/) | HTTPS через власний потік TLS1.3 |
| 29 | [29_2018_HTTP2_ALPN](29_2018_HTTP2_ALPN/) | HTTP/2 через TLS1.3 з ALPN h2 |
| 30 | [30_2021_HTTP3_QUIC_KEYS](30_2021_HTTP3_QUIC_KEYS/) | QUIC v1: захист пакетів і заголовків |
| 31 | [31_2021_HTTP3_QUIC_VARINT](31_2021_HTTP3_QUIC_VARINT/) | QUIC: цілі числа змінної довжини |
| 32 | [32_2021_HTTP3_QUIC_PACKETS](32_2021_HTTP3_QUIC_PACKETS/) | QUIC: формат пакетів і connection ID |
| 33 | [33_2021_HTTP3_QUIC_OFFSETS](33_2021_HTTP3_QUIC_OFFSETS/) | QUIC: збирання фрагментів потоків за offset |
| 34 | [34_2021_HTTP3_QUIC_ACK](34_2021_HTTP3_QUIC_ACK/) | QUIC: підтвердження діапазонів пакетів |
| 35 | [35_2021_HTTP3_QUIC_PARAMETERS](35_2021_HTTP3_QUIC_PARAMETERS/) | QUIC: транспортні параметри |
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
dotnet run --project 42_2022_HTTP3_QUIC_RECOVERY
curl --http1.0 -i http://127.0.0.1:8080/
curl --http1.0 -I http://127.0.0.1:8080/
curl --http1.1 --raw -i http://127.0.0.1:8080/chunks
```

Команди сервера і клієнта запускаються в різних терміналах.
GET/HEAD, `/`, `/chunks`, 404 для інших шляхів. HTTP/1.1 вимагає єдиного
непорожнього Host, повторно використовує TCP; `Connection: close` завершує сеанс.
На `/chunks` видно дві частини і кінцевий нуль. Ненульове тіло запиту
відхиляється з 400. Один TCP-клієнт за раз, таймаут 5 секунд.

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

Усі 42 проєкти збираються в Release без попереджень.
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
