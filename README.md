# HttpEvolution

Навчальне C#-демо: від HTTP/0.9 до HTTP/3, по одній можливості на коміт.
`main` — перший сервер на **22 рядки**. `chronological` — усі наступні кроки
в історичному порядку. `evolution` зберігає початкову історію розробки.

Мережеві API — тільки `TcpListener`, `TcpClient`, `UdpClient` і потоки байтів.
TLS, QUIC, HPACK і QPACK написані тут; криптографічні примітиви — з .NET BCL.
Без NuGet-залежностей, ASP.NET, `HttpListener`, `SslStream` і `System.Net.Quic`.
Потрібен **.NET 10 SDK**; усі режими слухають лише loopback.

## Показати перший крок

```sh
git switch main
dotnet run
# В іншому терміналі:
printf 'GET /\r\n' | nc 127.0.0.1 8080
# <h1>Hello, HTTP/0.9!</h1>
```

HTTP/0.9: один рядок GET; відповідь — HTML без статусу і заголовків,
після неї сервер закриває TCP-з'єднання. `/missing` повертає HTML «Not found».

## Пройти історію

```sh
git switch chronological
git log --reverse --oneline
# Будь-який коміт можна запустити: git switch --detach COMMIT; dotnet run
# Повернення до повної версії: git switch chronological
```

Кожен навчальний коміт збирається окремо. Теги позначають готові етапи;
дати — поява протоколу або публікація специфікації, не дати комітів.

| Рік | Тег для `git switch --detach TAG` | Що показати |
| --- | --- | --- |
| 1991 | `step-1991-http09` | GET → HTML → закриття TCP |
| 1996 | `step-1996-http10` | Статус, заголовки, Content-Length, HEAD |
| 1996 | `step-1996-ssl3` | HTTPS: RSA, Finished, MAC/3DES-CBC, ChangeCipherSpec |
| 1997 | `step-1997-http11` | Host, keep-alive, chunked |
| 1999 | `step-1999-tls10` | Перехід SSL3 → TLS1.0: PRF, HMAC, Finished |
| 2002 | `step-2002-tls-aes` | AES-CBC замість 3DES, сумісність з OpenSSL |
| 2015 | `step-2015-http2` | Кадри, потоки, HPACK/Huffman, flow control |
| 2018 | `step-2018-tls13` | ECDH/P-256, ECDSA, HKDF, AES-GCM, HTTPS |
| 2022 | `step-2022-http3` | QUIC/UDP, TLS1.3, QPACK, потоки, ACK і повтори |

SSL2 згаданий як попередник SSL3; його реалізації немає. TLS1.1/1.2 також
пропущені: це вибрані навчальні кроки, а не повна реалізація всіх версій.
HTTPS — HTTP усередині захищеного каналу, а не окрема версія HTTP.

## HTTP/1.x

```sh
dotnet run
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
dotnet run -- --ssl3
# В іншому терміналі підстав надрукований SPKI pin:
dotnet run -- --ssl3-client PIN

# Аналогічно для TLS1.0:
dotnet run -- --tls10
dotnet run -- --tls10-client PIN
```

SSL3 слухає TCP:8442, TLS1.0 — TCP:8441. Власний `TcpClient` перевіряє pin
сертифіката і Finished; ці старі протоколи використовуються лише як історичне демо.
Сертифікат тимчасовий, RSA/SHA-1; приватний ключ існує лише в пам'яті.
TLS1.0 також приймає незалежний OpenSSL-клієнт із AES128-SHA:

```sh
printf 'GET / HTTP/1.0\r\n\r\n' | openssl s_client -quiet -tls1 -cipher 'AES128-SHA:@SECLEVEL=0' -connect 127.0.0.1:8441
```

У навчальній історії окремо додано AES (2002) і початкову renegotiation indication
(2010). Повторного handshake немає. Сучасний режим використовує TLS1.3:

```sh
dotnet run -- --tls
curl -k --http1.1 -i https://localhost:8443/
```

TLS1.3: AES-128-GCM-SHA256, P-256, ECDSA, ALPN, HelloRetryRequest.
`-k` допускає тимчасовий self-signed сертифікат демо; resumption/0-RTT/KeyUpdate немає.

## HTTP/2

```sh
dotnet run -- --h2
curl --http2-prior-knowledge -i http://127.0.0.1:8081/
# Або HTTP/2 через TLS1.3, ALPN h2:
dotnet run -- --h2 --tls
curl -k --http2 -I https://localhost:8444/
```

Потрібен curl із підтримкою HTTP/2. Preface, SETTINGS/ACK, PING, GET/HEAD,
непарні stream ID, HPACK із динамічною таблицею і Huffman, CONTINUATION,
WINDOW_UPDATE та RST_STREAM. Тіло запиту не підтримується.

## HTTP/3

```sh
dotnet run -- --h3
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

Release build без попереджень; кожен коміт навчальної гілки збирається.
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
