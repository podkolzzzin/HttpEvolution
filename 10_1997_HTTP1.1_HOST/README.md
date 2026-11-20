# 10 · 1997 · HTTP/1.1 · HOST

Обов’язковий Host для HTTP/1.1.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../09_1996_HTTP1.0_SSL3_CLIENT .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
