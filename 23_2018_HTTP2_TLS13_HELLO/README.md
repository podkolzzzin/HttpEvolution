# 23 · 2018 · HTTP/2 · TLS13_HELLO

TLS1.3: ClientHello та P-256 key share.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h2
```

Це підготовчий крок TLS1.3; HTTP/2 через TCP уже працює. HTTPS із TLS1.3 доступний з етапу 28.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../22_2018_HTTP2_TLS13_KEYS/project project
```

[Усі етапи та клієнтські команди](../README.md).
