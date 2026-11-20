# 29 · 2018 · HTTP/2 · ALPN

HTTP/2 через TLS1.3 з ALPN h2.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі:

```sh
curl -k --http2 -i https://localhost:8444/
```

Код, доданий або змінений на цьому етапі:

- [Http2.cs](Http2.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../21_2015_HTTP2_CONTINUATION/Http2.cs Http2.cs
diff -u ../28_2018_HTTP2_TLS13_HTTPS/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
