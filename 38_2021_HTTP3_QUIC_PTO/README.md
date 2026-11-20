# 38 · 2021 · HTTP/3 · QUIC_PTO

QUIC: облік ACK та probe timeout.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h2
```

Це підготовчий крок QUIC/HTTP/3; команда вище запускає готовий HTTP/2. Повна відповідь HTTP/3 доступна з етапу 41.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../37_2021_HTTP3_QUIC_UDP/project project
```

[Усі етапи та клієнтські команди](../README.md).
