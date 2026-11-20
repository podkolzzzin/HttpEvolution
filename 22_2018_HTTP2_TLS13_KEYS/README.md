# 22 · 2018 · HTTP/2 · TLS13_KEYS

TLS1.3: ключі трафіку та захист записів.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h2
```

Це підготовчий крок TLS1.3; HTTP/2 через TCP уже працює. HTTPS із TLS1.3 доступний з етапу 28.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../21_2015_HTTP2_CONTINUATION .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
