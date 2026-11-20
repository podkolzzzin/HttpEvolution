# 41 · 2022 · HTTP/3 · RESPONSE

HTTP/3: GET/HEAD через QUIC, HEADERS/DATA/FIN.

Проєкт запускається з цієї папки; linked files видно в IDE поруч із локальним кодом:

```sh
dotnet run -- --h3
```

Код, доданий або змінений на цьому етапі:

- [Http3.cs](Http3.cs).
- [QuicServer.cs](QuicServer.cs).
- [QuicSession.cs](QuicSession.cs).

Порівняти фактичний код із попереднім кроком (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http3.cs
diff -u ../37_2021_HTTP3_QUIC_UDP/QuicServer.cs QuicServer.cs
diff -u ../40_2022_HTTP3_QUIC_STREAMS/QuicSession.cs QuicSession.cs
```

Спільні допоміжні файли зберігаються в [`common/`](../common/).
Потрібні попередні версії інших файлів проєкт підключає безпосередньо з етапів,
де вони вперше з’явилися. Повний склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).
Нову історичну версію зберігай у новому етапі, залишаючи попередню на місці.

[Усі етапи та клієнтські команди](../README.md).
