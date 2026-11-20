# 40 · 2022 · HTTP/3 · QUIC_STREAMS

QUIC для HTTP/3: потоки й flow control.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [QuicSession.cs](QuicSession.cs).
- [QuicStreams.cs](../common/QuicStreams.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../39_2022_HTTP3_QPACK/Program.cs Program.cs
diff -u ../36_2021_HTTP3_QUIC_TLS/QuicSession.cs QuicSession.cs
diff -u /dev/null ../common/QuicStreams.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
