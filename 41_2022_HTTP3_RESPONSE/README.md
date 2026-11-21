# 41 · 2022 · HTTP/3 · RESPONSE

HTTP/3: GET/HEAD через QUIC, HEADERS/DATA/FIN.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт HTTP/3 та SPKI pin описані в [головному README](../README.md#http3).

Код, доданий або змінений на цьому етапі:

- [Http3Connection.cs](Http3Connection.cs).
- [Program.cs](Program.cs).
- [QuicServer.cs](QuicServer.cs).
- [QuicSession.cs](QuicSession.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http3Connection.cs
diff -u ../40_2022_HTTP3_QUIC_STREAMS/Program.cs Program.cs
diff -u ../37_2021_HTTP3_QUIC_UDP/QuicServer.cs QuicServer.cs
diff -u ../40_2022_HTTP3_QUIC_STREAMS/QuicSession.cs QuicSession.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
