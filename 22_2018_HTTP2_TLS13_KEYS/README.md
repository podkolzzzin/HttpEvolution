# 22 · 2018 · HTTP/2 · TLS13_KEYS

TLS1.3: ключі трафіку та захист записів.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Консольне демо виконує приклад цього етапу, друкує результат і завершується.

Це окрема частина TLS1.3 у пам’яті; HTTPS-сервер доступний з етапу 28.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [TlsCrypto.cs](../common/TlsCrypto.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../21_2015_HTTP2_CONTINUATION/Program.cs Program.cs
diff -u /dev/null ../common/TlsCrypto.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
