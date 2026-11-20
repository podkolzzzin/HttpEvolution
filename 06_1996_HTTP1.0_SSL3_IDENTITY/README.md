# 06 · 1996 · HTTP/1.0 · SSL3_IDENTITY

SSL3: сертифікат RSA/SHA-1.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Це підготовка SSL3: приклад працює в пам’яті. HTTPS-сервер доступний з етапу 08.

Код, доданий або змінений на цьому етапі:

- [LegacyIdentity.cs](../common/LegacyIdentity.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null ../common/LegacyIdentity.cs
diff -u ../05_1996_HTTP1.0_SSL3_MESSAGES/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
