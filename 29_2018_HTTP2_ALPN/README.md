# 29 · 2018 · HTTP/2 · ALPN

HTTP/2 через TLS1.3 з ALPN h2.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h2 --tls
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../28_2018_HTTP2_TLS13_HTTPS/project project
```

[Усі етапи та клієнтські команди](../README.md).
