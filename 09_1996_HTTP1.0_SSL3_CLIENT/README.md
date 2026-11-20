# 09 · 1996 · HTTP/1.0 · SSL3_CLIENT

SSL3: TcpClient із перевіркою SPKI pin.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run -- --ssl3-client PIN
```

Код, доданий або змінений на цьому етапі:

- [LegacySsl.Client.cs](LegacySsl.Client.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null LegacySsl.Client.cs
diff -u ../08_1996_HTTP1.0_SSL3_HTTPS/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
