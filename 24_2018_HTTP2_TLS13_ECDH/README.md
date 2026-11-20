# 24 · 2018 · HTTP/2 · TLS13_ECDH

TLS1.3: узгодження секретів ECDH/P-256.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h2
```

Це підготовчий крок TLS1.3; HTTP/2 через TCP уже працює. HTTPS із TLS1.3 доступний з етапу 28.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../23_2018_HTTP2_TLS13_HELLO .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
