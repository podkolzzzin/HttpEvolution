# 12 · 1997 · HTTP/1.1 · CHUNKED

Передавання відповіді частинами: chunked.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../11_1997_HTTP1.1_KEEP_ALIVE .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
