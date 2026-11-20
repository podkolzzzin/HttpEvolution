# 24 · 2018 · HTTP/2 · TLS13_ECDH

TLS1.3: узгодження секретів ECDH/P-256.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h2
```

Це підготовчий крок TLS1.3; HTTP/2 через TCP уже працює. HTTPS із TLS1.3 доступний з етапу 28.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../23_2018_HTTP2_TLS13_HELLO/project project
```

[Усі етапи та клієнтські команди](../README.md).
