# 01 · 1991 · HTTP/0.9 · GET

GET → HTML → закриття TCP.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі:

```sh
printf 'GET /\r\n' | nc 127.0.0.1 8080
```

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
