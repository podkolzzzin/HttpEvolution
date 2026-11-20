# 08 · 1996 · HTTP/1.0 · SSL3_HTTPS

HTTPS через власний SSL3 handshake.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Код, доданий або змінений на цьому етапі:

- [LegacySsl.cs](LegacySsl.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null LegacySsl.cs
diff -u ../07_1996_HTTP1.0_SSL3_STREAM/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
