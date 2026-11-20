# 08 · 1996 · HTTP/1.0 · SSL3_HTTPS

HTTPS через власний SSL3 handshake.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --ssl3
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../07_1996_HTTP1.0_SSL3_STREAM/project project
```

[Усі етапи та клієнтські команди](../README.md).
