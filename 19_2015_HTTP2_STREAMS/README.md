# 19 · 2015 · HTTP/2 · STREAMS

HTTP/2: відповіді GET/HEAD у потоках.

З’єднання етапу 16 обробляє кадри керування. Цей етап додає HPACK-стан з’єднання та відповіді на GET/HEAD.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі:

```sh
curl --http2-prior-knowledge -i http://127.0.0.1:8081/
```

Код, доданий або змінений на цьому етапі:

- [Http2StreamConnection.cs](Http2StreamConnection.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http2StreamConnection.cs
diff -u ../17_2015_HTTP2_HPACK/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
