# 21 · 2015 · HTTP/2 · CONTINUATION

HTTP/2: збирання блоків CONTINUATION.

Будує на етапі 20. Збирає HEADERS і CONTINUATION одного потоку в один блок до декодування HPACK.

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

- [Http2ContinuationConnection.cs](Http2ContinuationConnection.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http2ContinuationConnection.cs
diff -u ../20_2015_HTTP2_FLOW_CONTROL/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
