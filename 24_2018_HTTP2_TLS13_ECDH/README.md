# 24 · 2018 · HTTP/2 · TLS13_ECDH

TLS1.3: узгодження секретів ECDH/P-256.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Це окрема частина TLS1.3 у пам’яті; HTTPS-сервер доступний з етапу 28.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [TlsHandshake.cs](TlsHandshake.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../23_2018_HTTP2_TLS13_HELLO/Program.cs Program.cs
diff -u /dev/null TlsHandshake.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
