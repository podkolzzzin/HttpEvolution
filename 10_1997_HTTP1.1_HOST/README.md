# 10 · 1997 · HTTP/1.1 · HOST

Обов’язковий Host для HTTP/1.1.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі:

```sh
curl --http1.1 -i http://127.0.0.1:8080/
```

Код, доданий або змінений на цьому етапі:

- [Http11HostConnection.cs](Http11HostConnection.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http11HostConnection.cs
diff -u ../09_1996_HTTP1.0_SSL3_CLIENT/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
