# 32 · 2021 · HTTP/3 · QUIC_PACKETS

QUIC: формат пакетів і connection ID.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h2
```

Це підготовчий крок QUIC/HTTP/3; команда вище запускає готовий HTTP/2. Повна відповідь HTTP/3 доступна з етапу 41.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../31_2021_HTTP3_QUIC_VARINT .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
