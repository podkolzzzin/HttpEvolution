# 28 · 2018 · HTTP/2 · TLS13_HTTPS

HTTPS через власний потік TLS1.3.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --tls
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../27_2018_HTTP2_TLS13_RECORDS .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
