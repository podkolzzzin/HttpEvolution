# common

18 допоміжних файлів, які побайтово однакові в усіх етапах, де вони присутні.
Тут зберігається одна копія кожного такого файлу. `Program.cs`, `Http2.cs` і `Http3.cs`
залишаються біля проєктів, як і всі файли, що змінюються в історії.

Кожен `.csproj` явно підключає лише потрібні на цьому етапі файли, наприклад:

```xml
<Compile Include="../common/Wire.cs" Link="common/Wire.cs" />
```

`Include` задає шлях до вихідного файлу, `Link` показує його в IDE у папці `common`.
Це спільні вихідні файли, які компілюються всередині кожного проєкту.
Набір підключень розширюється в міру проходження історії; ранні етапи не отримують
класів із наступних кроків. Вміст C#-файлів при перенесенні збережено без змін.

| Файл | Перший етап | Кількість проєктів |
| --- | --- | --- |
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
| [TlsCrypto.cs](TlsCrypto.cs) | 22 | 21 |
| [TlsHello.cs](TlsHello.cs) | 23 | 20 |
| [TlsRecords.cs](TlsRecords.cs) | 27 | 16 |
| [Wire.cs](Wire.cs) | 04 | 39 |

[Усі етапи й команди запуску](../README.md).
