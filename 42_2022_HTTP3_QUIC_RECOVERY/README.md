# 42 · 2022 · HTTP/3 · QUIC_RECOVERY

QUIC для HTTP/3: повтори з новими packet number.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h3
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../41_2022_HTTP3_RESPONSE/project project
```

[Усі етапи та клієнтські команди](../README.md).
