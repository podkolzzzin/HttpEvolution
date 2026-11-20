# 26 · 2018 · HTTP/2 · TLS13_FINISHED

TLS1.3: автентифікація та Finished.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h2
```

Це підготовчий крок TLS1.3; HTTP/2 через TCP уже працює. HTTPS із TLS1.3 доступний з етапу 28.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../25_2018_HTTP2_TLS13_IDENTITY/project project
```

[Усі етапи та клієнтські команди](../README.md).
