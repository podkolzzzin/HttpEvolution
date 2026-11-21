# 39 · 2022 · HTTP/3 · QPACK

HTTP/3: статичне кодування заголовків QPACK. Формати цілих чисел і рядків спільні з HPACK.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [Qpack.cs](../common/HeaderCompression/Qpack.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../38_2021_HTTP3_QUIC_PTO/Program.cs Program.cs
diff -u /dev/null ../common/HeaderCompression/Qpack.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
