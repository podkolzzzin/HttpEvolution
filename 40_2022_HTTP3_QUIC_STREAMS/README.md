# 40 · 2022 · HTTP/3 · QUIC_STREAMS

QUIC для HTTP/3: потоки й flow control.

Проєкт запускається з цієї папки; linked files видно в IDE поруч із локальним кодом:

```sh
dotnet run -- --h2
```

Це підготовчий крок QUIC/HTTP/3; команда вище запускає готовий HTTP/2. Повна відповідь HTTP/3 доступна з етапу 41.

Код, доданий або змінений на цьому етапі:

- [QuicSession.cs](QuicSession.cs).
- [QuicStreams.cs](../common/QuicStreams.cs).

Порівняти фактичний код із попереднім кроком (код завершення 1 означає відмінності):

```sh
diff -u ../36_2021_HTTP3_QUIC_TLS/QuicSession.cs QuicSession.cs
diff -u /dev/null ../common/QuicStreams.cs
```

Спільні допоміжні файли зберігаються в [`common/`](../common/).
Потрібні попередні версії інших файлів проєкт підключає безпосередньо з етапів,
де вони вперше з’явилися. Повний склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).
Нову історичну версію зберігай у новому етапі, залишаючи попередню на місці.

[Усі етапи та клієнтські команди](../README.md).
