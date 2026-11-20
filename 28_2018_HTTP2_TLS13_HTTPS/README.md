# 28 · 2018 · HTTP/2 · TLS13_HTTPS

HTTPS через власний потік TLS1.3.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі:

```sh
curl -k --http1.1 -i https://localhost:8443/
```

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [TlsStream.Tls13.cs](TlsStream.Tls13.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../27_2018_HTTP2_TLS13_RECORDS/Program.cs Program.cs
diff -u /dev/null TlsStream.Tls13.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
