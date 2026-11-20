# 16 · 2015 · HTTP/2 · FRAMES

HTTP/2: preface, SETTINGS і кадри через TCP.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Цей етап обробляє SETTINGS/PING; GET/HEAD з’являються на етапі 19.

Preface і порожній SETTINGS в іншому терміналі; відповідь у hex:

```sh
printf 'PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n\000\000\000\004\000\000\000\000\000' | nc -N 127.0.0.1 8081 | od -An -tx1
```

Код, доданий або змінений на цьому етапі:

- [Http2.cs](Http2.cs).
- [Http2Frames.cs](../common/Http2Frames.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http2.cs
diff -u /dev/null ../common/Http2Frames.cs
diff -u ../15_2010_HTTP1.1_TLS_RENEGOTIATION/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
