# 12 · 1997 · HTTP/1.1 · CHUNKED

Передавання відповіді частинами: chunked.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі:

```sh
curl --http1.1 --raw -i http://127.0.0.1:8080/chunks
```

Код, доданий або змінений на цьому етапі:

- [Http11ChunkedConnection.cs](Http11ChunkedConnection.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http11ChunkedConnection.cs
diff -u ../11_1997_HTTP1.1_KEEP_ALIVE/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
