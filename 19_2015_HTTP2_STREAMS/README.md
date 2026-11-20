# 19 · 2015 · HTTP/2 · STREAMS

HTTP/2: відповіді GET/HEAD у потоках.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h2
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../18_2015_HTTP2_HUFFMAN/project project
```

[Усі етапи та клієнтські команди](../README.md).
