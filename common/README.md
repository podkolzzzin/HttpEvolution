# common

22 спільні допоміжні файли. Кожен проєкт явно підключає лише потрібний
йому історичний набір через `Compile Include` і `Link`:

```xml
<Compile Include="../common/Wire.cs" Link="common/Wire.cs" />
```

`Include` задає шлях до вихідного файлу, `Link` показує його в IDE у папці `common`.
Ці файли компілюються всередині кожного проєкту. Додаткова бібліотека не потрібна.

`TlsStream.cs` містить спільну реалізацію потоку. TLS1.3 handshake підключається
окремою частиною класу з [етапу 28](../28_2018_HTTP2_TLS13_HTTPS/TlsStream.Tls13.cs).
`TlsCertificate.cs` створює сертифікат і підписи. Властивість SPKI pin додає
окрема частина класу з [етапу 37](../37_2021_HTTP3_QUIC_UDP/TlsCertificate.Pin.cs).
`Http2Frames.cs` читає й записує кадри; логіка обробки HTTP/2 залишається в етапах.

`DemoClientHello` формує ClientHello з тимчасовим P-256 ключем для чотирьох
консольних TLS/QUIC-демо. У кожного етапу власний локальний `Program.cs`;
точки входу не підключаються як linked files.

Історичні версії інших класів зберігаються один раз — у першому етапі, де вони
з’явилися. Наступні проєкти явно підключають потрібну версію з цієї папки.
Кожне посилання веде безпосередньо до C#-файлу; ранні етапи не залежать від пізніших.

| Файл | Перший етап | Кількість проєктів |
| --- | --- | --- |
| [DemoClientHello.cs](DemoClientHello.cs) | 23 | 4 |
| [Http2Frames.cs](Http2Frames.cs) | 16 | 27 |
| [Huffman.cs](Huffman.cs) | 18 | 25 |
| [LegacyIdentity.cs](LegacyIdentity.cs) | 06 | 37 |
| [Qpack.cs](Qpack.cs) | 39 | 4 |
| [QuicAcks.cs](QuicAcks.cs) | 34 | 9 |
| [QuicBuffer.cs](QuicBuffer.cs) | 33 | 10 |
| [QuicData.cs](QuicData.cs) | 31 | 12 |
| [QuicKeys.cs](QuicKeys.cs) | 30 | 13 |
| [QuicPacket.cs](QuicPacket.cs) | 32 | 11 |
| [QuicParameters.cs](QuicParameters.cs) | 35 | 8 |
| [QuicRecovery.cs](QuicRecovery.cs) | 38 | 5 |
| [QuicStreams.cs](QuicStreams.cs) | 40 | 3 |
| [RecordLayer.cs](RecordLayer.cs) | 05 | 38 |
| [Ssl3Crypto.cs](Ssl3Crypto.cs) | 04 | 39 |
| [Tls10Crypto.cs](Tls10Crypto.cs) | 13 | 30 |
| [TlsCertificate.cs](TlsCertificate.cs) | 25 | 18 |
| [TlsCrypto.cs](TlsCrypto.cs) | 22 | 21 |
| [TlsHello.cs](TlsHello.cs) | 23 | 20 |
| [TlsRecords.cs](TlsRecords.cs) | 27 | 16 |
| [TlsStream.cs](TlsStream.cs) | 07 | 36 |
| [Wire.cs](Wire.cs) | 04 | 39 |

[Усі етапи й команди запуску](../README.md).
