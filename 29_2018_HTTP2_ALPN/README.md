# 29 · 2018 · HTTP/2 · ALPN

HTTP/2 через TLS1.3 з ALPN h2.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h2 --tls
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../28_2018_HTTP2_TLS13_HTTPS .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
