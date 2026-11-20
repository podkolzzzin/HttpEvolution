# 21 · 2015 · HTTP/2 · CONTINUATION

HTTP/2: збирання блоків CONTINUATION.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h2
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../20_2015_HTTP2_FLOW_CONTROL .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
