# 02 · 1996 · HTTP/1.0 · HEADERS

Статус, заголовки та Content-Length.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі:

```sh
curl --http1.0 -i http://127.0.0.1:8080/
```

Код, доданий або змінений на цьому етапі:

- [Http10Connection.cs](Http10Connection.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http10Connection.cs
diff -u ../01_1991_HTTP0.9_GET/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
