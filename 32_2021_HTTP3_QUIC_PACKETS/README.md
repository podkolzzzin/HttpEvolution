# 32 · 2021 · HTTP/3 · QUIC_PACKETS

QUIC: формат пакетів і connection ID.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [QuicPacket.cs](../common/QuicPacket.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../31_2021_HTTP3_QUIC_VARINT/Program.cs Program.cs
diff -u /dev/null ../common/QuicPacket.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
