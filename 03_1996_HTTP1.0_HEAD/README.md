# 03 · 1996 · HTTP/1.0 · HEAD

HEAD: заголовки без тіла відповіді.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі:

```sh
curl --http1.0 -I http://127.0.0.1:8080/
```

Код, доданий або змінений на цьому етапі:

- [Http10HeadConnection.cs](Http10HeadConnection.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http10HeadConnection.cs
diff -u ../02_1996_HTTP1.0_HEADERS/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
