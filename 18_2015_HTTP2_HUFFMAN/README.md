# 18 · 2015 · HTTP/2 · HUFFMAN

HPACK: декодування рядків Huffman.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Код, доданий або змінений на цьому етапі:

- [Hpack.cs](Hpack.cs).
- [Huffman.cs](../common/Huffman.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../17_2015_HTTP2_HPACK/Hpack.cs Hpack.cs
diff -u /dev/null ../common/Huffman.cs
diff -u ../17_2015_HTTP2_HPACK/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
