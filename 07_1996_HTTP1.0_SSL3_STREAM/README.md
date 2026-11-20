# 07 · 1996 · HTTP/1.0 · SSL3_STREAM

SSL3: розшифровані записи як потік байтів.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Це підготовка SSL3: приклад працює в пам’яті. HTTPS-сервер доступний з етапу 08.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [TlsStream.cs](../common/TlsStream.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../06_1996_HTTP1.0_SSL3_IDENTITY/Program.cs Program.cs
diff -u /dev/null ../common/TlsStream.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
