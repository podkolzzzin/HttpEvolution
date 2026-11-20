# 36 · 2021 · HTTP/3 · QUIC_TLS

QUIC: TLS handshake на рівнях шифрування.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

У пам’яті формується справжній захищений QUIC Initial із ClientHello та відповідь сервера; UDP-сервер доступний з етапу 37.

Код, доданий або змінений на цьому етапі:

- [DemoClientHello.cs](../common/DemoClientHello.cs).
- [Program.cs](Program.cs).
- [QuicSession.cs](QuicSession.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null ../common/DemoClientHello.cs
diff -u ../35_2021_HTTP3_QUIC_PARAMETERS/Program.cs Program.cs
diff -u /dev/null QuicSession.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
