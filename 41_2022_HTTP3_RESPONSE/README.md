# 41 · 2022 · HTTP/3 · RESPONSE

HTTP/3: GET/HEAD через QUIC, HEADERS/DATA/FIN.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h3
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../40_2022_HTTP3_QUIC_STREAMS/project project
```

[Усі етапи та клієнтські команди](../README.md).
