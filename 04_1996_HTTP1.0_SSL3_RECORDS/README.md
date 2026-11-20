# 04 · 1996 · HTTP/1.0 · SSL3_RECORDS

SSL3: MAC і шифрування записів 3DES-CBC.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Це підготовка SSL3: приклад працює в пам’яті. HTTPS-сервер доступний з етапу 08.

Код, доданий або змінений на цьому етапі:

- [LegacyCipher.cs](LegacyCipher.cs).
- [Program.cs](Program.cs).
- [Ssl3Crypto.cs](../common/Ssl3Crypto.cs).
- [Wire.cs](../common/Wire.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null LegacyCipher.cs
diff -u ../03_1996_HTTP1.0_HEAD/Program.cs Program.cs
diff -u /dev/null ../common/Ssl3Crypto.cs
diff -u /dev/null ../common/Wire.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
