# 20 · 2015 · HTTP/2 · FLOW_CONTROL

HTTP/2: вікна flow control.

Будує на етапі 19. Додає вікна з’єднання й потоків, чергу тіл відповідей і обробку WINDOW_UPDATE.

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

- [Http2FlowControlConnection.cs](Http2FlowControlConnection.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http2FlowControlConnection.cs
diff -u ../19_2015_HTTP2_STREAMS/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
