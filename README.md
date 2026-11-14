# HttpEvolution

Компактне C#-демо еволюції HTTP: одна можливість на коміт.
Гілка `main` — перший крок HTTP/0.9; `evolution` — наступні кроки.
**У цій гілці є HTTP/1.x та базове cleartext HTTP/2.**
Мережа: `TcpListener` → `TcpClient` → потік байтів; без ASP.NET, Kestrel,
`HttpListener`, `HttpClient` і сторонніх пакетів.

## Запуск

Потрібен .NET 10 SDK. У папці репозиторію:

```sh
git switch evolution
dotnet run
```

В іншому терміналі (Linux/macOS, потрібен `nc`):

```sh
printf 'GET /\r\n' | nc 127.0.0.1 8080
# <h1>Hello, HTTP!</h1>

printf 'GET /missing\r\n' | nc 127.0.0.1 8080
# <h1>Not found</h1>
```

Порівняй із версійованим запитом:

```sh
curl --http1.0 -i http://127.0.0.1:8080/
curl --http1.0 -I http://127.0.0.1:8080/
curl --http1.1 -i http://127.0.0.1:8080/
```

## Що видно на дроті

Запит 0.9: ASCII-рядок `GET /\r\n` (також приймається LF).
Відповідь на нього: лише HTML; немає версії, статусу чи заголовків.
Запит 1.0: `GET / HTTP/1.0\r\n\r\n`; відповідь починається з
`HTTP/1.0 200 OK`, має `Content-Type`, `Content-Length` та порожній рядок перед HTML.
`HEAD / HTTP/1.0` повертає ті самі заголовки, включно з довжиною GET-відповіді,
але не передає тіло.
Запит 1.1 отримує відповідь 1.1. Для нього обов'язковий єдиний непорожній
`Host`; відсутній, порожній або дубльований Host дає `400 Bad Request`.
HTTP/1.1 повторно використовує TCP-з'єднання; межа відповіді — `Content-Length`.
`Connection: close` завершує його після відповіді. Запити 0.9 і 1.0 завжди закривають з'єднання.
Покажи дві відповіді в одному з'єднанні:

```sh
printf 'GET / HTTP/1.1\r\nHost: localhost\r\n\r\nGET /missing HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n' | nc 127.0.0.1 8080
```

Демо обслуговує GET/HEAD без тіла запиту. Ненульовий `Content-Length` або
будь-який `Transfer-Encoding` відхиляються з 400 і закриттям з'єднання,
щоб тіло не стало наступним запитом. Незавершений блок заголовків не обробляється.
Сторінки — `/` і `/chunks`; інші адреси отримують HTML про відсутність сторінки,
з кодом 404 у відповіді 1.0. Невідомі методи й порожні запити закриваються без відповіді.

Для стислості: один клієнт за раз, тільки loopback:8080, таймаут читання/запису
5 секунд, без обмеження довжини рядка. Це локальна демонстрація протоколу.

На `/chunks` HTTP/1.1 передає HTML двома частинами: шістнадцяткова довжина,
CRLF, байти частини, CRLF. Нульова частина `0\r\n\r\n` завершує тіло,
тому `Content-Length` тут відсутній. HEAD повертає лише заголовки; для 0.9/1.0
ця сама сторінка використовує звичайну відповідь без chunked.

```sh
curl --http1.1 --raw -i http://127.0.0.1:8080/chunks
```

## HTTP/2

`dotnet run -- --h2` слухає TCP:8081. Реалізовано preface, 9-байтовий
заголовок кадру, SETTINGS/ACK, PING/ACK та відповіді на GET/HEAD.
Це cleartext HTTP/2 із prior knowledge; шифрування додається окремо.
`Hpack.cs` окремо декодує індекси, літерали та динамічну таблицю заголовків;
приклади з Huffman і без нього перевірено за RFC 7541 C.3/C.4.
`Huffman.cs` відновлює канонічні коди з 257 довжин таблиці RFC; перевірено всі
256 байтів, EOS і padding. Відповідь має HEADERS із `:status` та DATA з HTML;
прапорець END_STREAM завершує відповідь. HEAD не має DATA.
Кожен запит має власний непарний stream ID. DATA враховує вікно з'єднання,
SETTINGS_INITIAL_WINDOW_SIZE та WINDOW_UPDATE; при нульовому вікні тіло чекає,
інші потоки й PING продовжують працювати. RST_STREAM прибирає очікувану відповідь.
HEADERS може продовжуватися кадрами CONTINUATION до END_HEADERS (до 64 KiB);
початковий HEADERS має END_STREAM, бо демо не приймає тіла запитів.
Інакше потік відхиляється через RST_STREAM, зберігаючи HPACK-стан з'єднання.

```sh
curl --http2-prior-knowledge -i http://127.0.0.1:8081/
curl --http2-prior-knowledge -I http://127.0.0.1:8081/
```

Для HTTP/2 усередині TLS 1.3:

```sh
dotnet run -- --h2 --tls
curl -k --http2 -i https://localhost:8444/
```

ALPN узгоджує `h2`; HTTP/2 використовує той самий потік, що й HTTPS/1.1.

## HTTPS / TLS 1.3

```sh
dotnet run -- --tls
# В іншому терміналі:
curl -k -i https://localhost:8443/
```

`-k` потрібен для тимчасового self-signed сертифіката демо. Підтримуються
TLS 1.3, AES-128-GCM-SHA256, P-256, ECDSA та ALPN http/1.1.
Це навчальна реалізація: без resumption, 0-RTT і KeyUpdate; записів на ключ — до 2^24.

`TlsCrypto.cs` додає HKDF-Expand-Label, Finished та AES-128-GCM.
Криптографічні примітиви — з .NET BCL; формат протоколу реалізовано вручну.
Ключі, Finished і зашифрований запис побайтово збігаються з RFC 8448;
змінений тег, заголовок або номер запису не проходять перевірку.
`TlsHello.cs` читає ClientHello, вибирає AES-128-GCM/P-256 і формує
ServerHello або HelloRetryRequest. Перевірено справжні ClientHello OpenSSL,
ALPN, відлуння session ID і відхилення обрізаних повідомлень.
`TlsHandshake.cs` узгоджує P-256, обчислює ECDH-секрет і ключі handshake.
Шлях із HelloRetryRequest використовує синтетичний `message_hash` у transcript.
Перевірено однакові секрети обох сторін із retry й без нього.
`TlsCertificate.cs` створює тимчасовий self-signed сертифікат для localhost
та підписує CertificateVerify; приватний ключ існує тільки в пам'яті процесу.
Handshake формує EncryptedExtensions, Certificate, CertificateVerify та Finished,
перевіряє клієнтський Finished і виводить окремі ключі application data.
При завершенні сеансу ключі handshake очищаються.
`TlsRecords.cs` кодує plaintext/encrypted записи, збирає розбиті handshake-повідомлення
та перевіряє AEAD і номери записів. `TlsStream.cs` виконує handshake й надає
HTTP-коду потік розшифрованих байтів. Перевірено HTTPS через curl/OpenSSL,
довіру до сертифіката, hostname, close_notify та відновлення після невдалого handshake.

## HTTP/3: QUIC у роботі

`QuicKeys.cs` додає ключі Initial, AES-GCM payload protection і AES header protection.
Клієнтський і серверний пакети побайтово збігаються з RFC 9001 Appendix A;
перевірено відновлення обрізаного packet number та переставлені пакети.
`QuicData.cs` читає й записує varint та вектори QUIC/HTTP/3; перевірено приклади RFC 9000
і межі всіх чотирьох ширин. `QuicPacket.cs` розбирає long/short headers і coalesced
пакети та кодує Initial/Handshake/1-RTT, включно з padding Initial до 1200 байтів.
`QuicBuffer.cs` збирає CRYPTO/STREAM-фрагменти за offset, усуває дублікати й перевіряє FIN.
`QuicAcks.cs` кодує й розбирає ACK-діапазони з пропусками та ECN.
UDP-транспорт, QUIC-потоки й HTTP/3 ще не підключені.

## Хронологія і стан

Дати показують історичну появу або публікацію специфікацій, а не версії .NET.
Кожен пункт із кількома можливостями розбивається на окремі короткі коміти.

| Період | Що додати |
| --- | --- |
| 1991 — HTTP/0.9 | **Готово:** GET → HTML → закриття TCP |
| 1995–1996 — HTTPS/SSL | Пояснити появу захищеного HTTP; SSL залишити історичним контекстом |
| 1996 — HTTP/1.0 | **Готово в evolution:** версія, статус, заголовки, Content-Length і HEAD |
| 1997 — HTTP/1.1 | **Готово в evolution:** версія, Host, keep-alive і chunked |
| 1999 — TLS 1.0 | Історичний перехід SSL → TLS; виконуване HTTPS — у кроці TLS 1.3 |
| 2015 — HTTP/2 | **Готово в evolution:** кадри, SETTINGS, HPACK/Huffman, GET/HEAD, вікна потоків і CONTINUATION; також h2 через TLS |
| 2018 — TLS 1.3 | **Готово в evolution:** handshake, сертифікат, ECDH, AES-GCM і HTTPS/1.1 |
| 2022 — HTTP/3 | QUIC поверх UDP з TLS 1.3; потоки; кадри; QPACK |

HTTPS — HTTP усередині захищеного каналу, а не окрема версія HTTP.
Для захищених етапів зберігаємо обмеження на мережеві API:
TLS-повідомлення й записи реалізуємо поверх `TcpClient`, використовуючи криптопримітиви BCL.
SSL та старі TLS описуємо історично; для виконуваного демо використовуватимемо сучасний TLS.
Сам `UdpClient` дає лише дейтаграми: HTTP/3 потребує ще QUIC, TLS і QPACK.
Власна реалізація цих шарів суттєво збільшить код; готовий QUIC теж розширить
перелік дозволених API. Звичайний обмін текстом через UDP не є HTTP/3.

## Першоджерела

- [HTTP/0.9 — W3C, 1991](https://www.w3.org/Protocols/HTTP/AsImplemented)
- [SSL 3.0 — історична специфікація](https://www.rfc-editor.org/rfc/rfc6101), [скасування SSL 3.0](https://www.rfc-editor.org/rfc/rfc7568)
- [HTTP/1.0](https://www.rfc-editor.org/rfc/rfc1945), [перший RFC HTTP/1.1](https://www.rfc-editor.org/rfc/rfc2068)
- [Поточна специфікація повідомлень HTTP/1.1](https://www.rfc-editor.org/rfc/rfc9112.html)
- [TLS 1.0](https://www.rfc-editor.org/rfc/rfc2246), [TLS 1.3](https://www.rfc-editor.org/rfc/rfc8446)
- [Контрольні приклади TLS 1.3](https://www.rfc-editor.org/rfc/rfc8448)
- [перший RFC HTTP/2](https://www.rfc-editor.org/rfc/rfc7540), [HTTP/3](https://www.rfc-editor.org/rfc/rfc9114)
