# 05 · 1996 · HTTP/1.0 · SSL3_MESSAGES

SSL3: повідомлення handshake та ChangeCipherSpec.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Це підготовка SSL3: приклад працює в пам’яті. HTTPS-сервер доступний з етапу 08.

Код, доданий або змінений на цьому етапі:

- [LegacyRecords.cs](LegacyRecords.cs).
- [Program.cs](Program.cs).
- [RecordLayer.cs](../common/RecordLayer.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null LegacyRecords.cs
diff -u ../04_1996_HTTP1.0_SSL3_RECORDS/Program.cs Program.cs
diff -u /dev/null ../common/RecordLayer.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
