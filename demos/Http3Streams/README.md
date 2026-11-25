# Нічна обсерваторія · HTTP/3 через Stream

Клієнт відкриває одне QUIC-з’єднання й одночасно запитує програму спостережень, Місяць і Сатурн. Кожен GET має власний двонапрямний `QuicStream`. У консолі видно ідентифікатор потоку, шлях, статус і текст відповіді.

## Запуск

З кореня репозиторію, з .NET 10 SDK і MsQuic:

```sh
dotnet run --project demos/Http3Streams
```

На macOS встанови `brew install libmsquic`. Профіль запуску вже містить стандартні шляхи Homebrew для Apple Silicon та Intel. На Linux потрібен пакет `libmsquic`. Для Windows бібліотека входить до підтримуваного середовища .NET. [Платформні залежності .NET QUIC](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/quic/quic-overview#platform-dependencies)

Демо саме вибирає вільний порт на loopback і завершує обидві сторони після відповідей. Для показу у двох терміналах запусти сервер на UDP-порту 8456:

```sh
dotnet run --project demos/Http3Streams -- server
```

Сервер надрукує готову команду клієнта з SHA-256 відбитком свого тимчасового сертифіката. Її можна запускати кілька разів. Для перевірки окремим клієнтом підійде curl із підтримкою HTTP/3:

```sh
curl --http3-only -ki https://127.0.0.1:8456/moon
```

## Де подивитися код

1. [Http3Client.GetAsync](Transport/Http3Client.cs) відкриває двонапрямний потік, пише запит і викликає `CompleteWrites()`. Це завершує надсилання запиту. Той самий потік залишається відкритим для читання відповіді.
2. [Http3Server](Transport/Http3Server.cs) приймає запити паралельно. Він передає шлях обробнику через [IRequestHandler](Application/IRequestHandler.cs). [Observatory](Application/Observatory.cs) відповідає за маршрути й програму спостережень.
3. [Http3Message](Protocol/Http3Message.cs) приймає звичайний `System.IO.Stream`. У мережі йому передають `System.Net.Quic.QuicStream`, у тестах — `MemoryStream`, який може видавати по одному байту. Код обробки HTTP-повідомлення в обох випадках той самий.
4. [Http3Session](Transport/Http3Session.cs) володіє QUIC-з’єднанням і одностороннім керівним потоком із SETTINGS. Кодек реалізує HEADERS, DATA, статичний QPACK і читання Huffman. Приклад обмежений HTTPS GET без тіла, фінальними текстовими відповідями та розміром кадру й тіла до 64 КіБ. [HTTP/3](https://www.rfc-editor.org/rfc/rfc9114.html) · [QPACK](https://www.rfc-editor.org/rfc/rfc9204.html)

## Тести

```sh
dotnet test demos/Http3Streams/Tests
```

На macOS передай тестовому процесу шлях до MsQuic:

```sh
dotnet test demos/Http3Streams/Tests -e DYLD_FALLBACK_LIBRARY_PATH="$(brew --prefix libmsquic)/lib"
```

Тести [кодека](Tests/MessageTests.cs) перевіряють байти кадрів, фрагментацію та UTF-8. У [мережевому тесті](Tests/LoopbackTests.cs) обробник призупиняє перший запит. Клієнт отримує відповідь на другому потоці, поки перший іще відкритий. Інший тест робить HTTP/3-запит стандартним `HttpClient`. Для цих двох тестів потрібен MsQuic.
