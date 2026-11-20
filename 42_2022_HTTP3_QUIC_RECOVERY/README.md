# 42 · 2022 · HTTP/3 · QUIC_RECOVERY

QUIC для HTTP/3: повтори з новими packet number.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h3
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../41_2022_HTTP3_RESPONSE .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
