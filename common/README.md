# common

Кожен проєкт підключає потрібні йому спільні файли через `Compile Include` і `Link`.

```xml
<Compile Include="../common/Wire/Wire.cs" Link="common/Wire/Wire.cs" />
```

`Include` задає шлях до вихідного файлу. `Link` показує його в тій самій підпапці
`common` в IDE. Файли компілюються всередині проєкту етапу.

| Папка | Вміст |
| --- | --- |
| [Demo](Demo/) | Аргументи командного рядка через System.CommandLine і ClientHello для консольних прикладів |
| [HeaderCompression](HeaderCompression/) | HPACK, QPACK, Huffman і спільні формати заголовків |
| [Http1](Http1/) | Цикл приймання TCP-з’єднань для HTTP/0.9 та HTTP/1.x |
| [Http2](Http2/) | Типи кадрів, їх читання й запис |
| [Quic](Quic/) | Пакети, ключі, ACK, транспортні параметри, потоки й відновлення |
| [Tls](Tls/) | Записи, шифрування, ідентифікація й потоки SSL/TLS |
| [Wire](Wire/) | Робота з цілими числами й векторами байтів для SSL/TLS та QUIC |

У кожного етапу власний `Program.cs`. Класи, що вводять нову поведінку,
зберігаються у відповідних етапах. Наприклад, HTTP/2 починається з
[кадрів і встановлення з’єднання](../16_2015_HTTP2_FRAMES/Http2Connection.cs),
далі додає [відповіді в потоках](../19_2015_HTTP2_STREAMS/Http2StreamConnection.cs),
[вікна](../20_2015_HTTP2_FLOW_CONTROL/Http2FlowControlConnection.cs),
[CONTINUATION](../21_2015_HTTP2_CONTINUATION/Http2ContinuationConnection.cs)
і [TLS з ALPN](../29_2018_HTTP2_ALPN/Http2Alpn.cs).
TLS 1.3 handshake та SPKI pin підключаються як окремі частини класів
з [етапу 28](../28_2018_HTTP2_TLS13_HTTPS/TlsStream.Tls13.cs) та
[етапу 37](../37_2021_HTTP3_QUIC_UDP/TlsCertificate.Pin.cs).

[Усі етапи й команди запуску](../README.md).
