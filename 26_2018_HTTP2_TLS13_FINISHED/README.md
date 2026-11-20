# 26 · 2018 · HTTP/2 · TLS13_FINISHED

TLS1.3: автентифікація та Finished.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Це окрема частина TLS1.3 у пам’яті; HTTPS-сервер доступний з етапу 28.

Client Finished обчислюється з узгодженого секрету всередині демо; мережевого клієнта немає.

Код, доданий або змінений на цьому етапі:

- [DemoClientHello.cs](../common/DemoClientHello.cs).
- [Program.cs](Program.cs).
- [TlsHandshake.cs](TlsHandshake.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u /dev/null ../common/DemoClientHello.cs
diff -u ../25_2018_HTTP2_TLS13_IDENTITY/Program.cs Program.cs
diff -u ../24_2018_HTTP2_TLS13_ECDH/TlsHandshake.cs TlsHandshake.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
