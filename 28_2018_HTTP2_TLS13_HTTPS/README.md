# 28 · 2018 · HTTP/2 · TLS13_HTTPS

HTTPS через власний потік TLS1.3.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --tls
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../27_2018_HTTP2_TLS13_RECORDS/project project
```

[Усі етапи та клієнтські команди](../README.md).
