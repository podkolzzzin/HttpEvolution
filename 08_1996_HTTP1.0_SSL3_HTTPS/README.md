# 08 · 1996 · HTTP/1.0 · SSL3_HTTPS

HTTPS через власний SSL3 handshake.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --ssl3
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../07_1996_HTTP1.0_SSL3_STREAM .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
