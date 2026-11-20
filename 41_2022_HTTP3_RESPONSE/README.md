# 41 · 2022 · HTTP/3 · RESPONSE

HTTP/3: GET/HEAD через QUIC, HEADERS/DATA/FIN.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h3
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../40_2022_HTTP3_QUIC_STREAMS .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
